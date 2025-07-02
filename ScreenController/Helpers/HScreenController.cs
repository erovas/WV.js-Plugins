using ScreenController.Win32;
using ScreenController.Win32.Enums;
using ScreenController.Win32.Structs;
using System.Runtime.InteropServices;

namespace ScreenController.Helpers
{
    internal static class HScreenController
    {
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        private const int SRCCOPY = 0x00CC0020;
        private const int DIB_RGB_COLORS = 0;
        private const int MONITOR_DPI_TYPE = 0; // MDT_EFFECTIVE_DPI
        private const int CURSOR_SHOWING = 0x00000001;

        private const uint DI_NORMAL = 0x0003;
        private const uint DI_IMAGE = 0x0002;
        private const uint DI_MASK = 0x0001;

        internal static IntPtr Handled {  get; set; }


        /// <summary>
        /// Obtiene información detallada de todas las pantallas
        /// </summary>
        /// <returns></returns>
        public static HScreenInfo[] GetAllScreensInfo()
        {
            List<HScreenInfo> screens = new List<HScreenInfo>();
            Dictionary<string, DEVMODE> devModes = new Dictionary<string, DEVMODE>();
            Dictionary<string, nint> monitorHandles = new Dictionary<string, nint>();

            // Obtener configuraciones de pantalla
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE { cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE)) };

            for (uint deviceIndex = 0; User32.EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0); deviceIndex++)
            {
                //if ((displayDevice.StateFlags & 0x00000001) != 0) // DISPLAY_DEVICE_ACTIVE
                if ((displayDevice.StateFlags & StateFlags.DISPLAY_DEVICE_ACTIVE) != 0) // DISPLAY_DEVICE_ACTIVE
                {
                    DEVMODE devMode = new DEVMODE { dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)) };

                    if (User32.EnumDisplaySettings(displayDevice.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                        devModes[displayDevice.DeviceName] = devMode;
                }
            }

            // Enumerar monitores físicos
            User32.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT rect, IntPtr data) =>
            {
                MONITORINFO mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO)) };
                if (User32.GetMonitorInfo(hMonitor, ref mi))
                {
                    DISPLAY_DEVICE device = new DISPLAY_DEVICE { cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE)) };

                    // Obtener DPI
                    uint dpiX = 96, dpiY = 96;
                    Shcore.GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE, out dpiX, out dpiY);

                    // Buscar dispositivo correspondiente
                    foreach (var devMode in devModes)
                    {
                        if (User32.EnumDisplayDevices(devMode.Key, 0, ref device, 0))
                        {
                            screens.Add(new HScreenInfo
                            {
                                DeviceName = devMode.Key,
                                DeviceID = device.DeviceID,
                                DeviceKey = device.DeviceKey,
                                DeviceString = device.DeviceString,
                                StateFlags = device.StateFlags,
                                MonitorArea = mi.rcMonitor,
                                WorkArea = mi.rcWork,
                                IsPrimary = (mi.dwFlags & 1) == 1,
                                BitsPerPixel = devModes[devMode.Key].dmBitsPerPel,
                                RefreshRate = devModes[devMode.Key].dmDisplayFrequency,
                                PixelWidth = devModes[devMode.Key].dmPelsWidth,
                                PixelHeight = devModes[devMode.Key].dmPelsHeight,
                                PositionX = devModes[devMode.Key].dmPositionX,
                                PositionY = devModes[devMode.Key].dmPositionY,
                                DpiX = dpiX,
                                DpiY = dpiY
                            });
                            break;
                        }
                    }
                }
                return true;
            };

            User32.EnumDisplayMonitors(nint.Zero, nint.Zero, callback, nint.Zero);

            return screens.ToArray();
        }

        /// <summary>
        /// Captura una región específica del escritorio
        /// </summary>
        /// <param name="region">Región a capturar (coordenadas absolutas)</param>
        /// <param name="excludeAppWindow">Si es true, excluye la ventana de la aplicación actual</param>
        /// <returns></returns>
        public static HScreenCapture CaptureRegion(RECT region, bool excludeAppWindow = false, bool includeCursor = false)
        {
            // Validar región
            RECT virtualBounds = GetVirtualScreenBounds();
            region = IntersectRects(region, virtualBounds);

            if (region.Width <= 0 || region.Height <= 0)
                return new HScreenCapture(nint.Zero, 0, 0, 0, 0);

            nint hdcScreen = Gdi32.CreateDC("DISPLAY", null, null, nint.Zero);
            nint hdcMem = Gdi32.CreateCompatibleDC(hdcScreen);

            // Crear estructura BITMAPINFO
            BITMAPINFO bmi = new BITMAPINFO
            {
                bmiHeader = new BITMAPINFOHEADER
                {
                    biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER)),
                    biWidth = region.Width,
                    biHeight = -region.Height, // Negativo para top-down
                    biPlanes = 1,
                    biBitCount = 32,
                    biCompression = 0, // BI_RGB
                    biSizeImage = 0
                }
            };


            // Crear DIB section
            nint bits;
            nint hBitmap = Gdi32.CreateDIBSection(hdcScreen, ref bmi, DIB_RGB_COLORS, out bits, nint.Zero, 0);

            // Seleccionar bitmap en el DC
            nint hOld = Gdi32.SelectObject(hdcMem, hBitmap);

            // Capturar región
            Gdi32.BitBlt(hdcMem, 0, 0, region.Width, region.Height, hdcScreen, region.Left, region.Top, SRCCOPY);

            // Dibujar cursor si es necesario
            if (includeCursor)
                DrawCursorOnCapture(hdcMem, region.Left, region.Top);

            // Limpiar
            Gdi32.SelectObject(hdcMem, hOld);
            Gdi32.DeleteDC(hdcMem);
            Gdi32.DeleteDC(hdcScreen);

            // Procesar exclusión de ventana
            if (excludeAppWindow)
                ExcludeAppWindowFromCapture(bits, region, region.Width * 4, region.Height);
            
            return new HScreenCapture(bits, region.Width, region.Height, region.Width * 4, 32);
        }

        public static HCursorInfo? GetCurrentCursorInfo()
        {
            CURSORINFO cursorInfo = new CURSORINFO
            {
                cbSize = Marshal.SizeOf(typeof(CURSORINFO))
            };

            if (!User32.GetCursorInfo(out cursorInfo)) 
                return null;

            ICONINFO iconInfo;

            if (!User32.GetIconInfo(cursorInfo.hCursor, out iconInfo)) 
                return null;

            // Obtener dimensiones del cursor
            BITMAP bmp = new BITMAP();
            Gdi32.GetObject(iconInfo.hbmColor, Marshal.SizeOf(bmp), ref bmp);

            return new HCursorInfo
            {
                Position = new POINT
                {
                    X = cursorInfo.ptScreenPos.X,
                    Y = cursorInfo.ptScreenPos.Y
                },
                Size = new SIZE
                {
                    cx = bmp.bmWidth,
                    cy = bmp.bmHeight
                },
                Handle = cursorInfo.hCursor,
                IsVisible = (cursorInfo.flags & CURSOR_SHOWING) != 0,
                HotSpot = new POINT
                {
                    X = iconInfo.xHotspot,
                    Y = iconInfo.yHotspot
                },
                CursorImage = GetCursorImageData(cursorInfo.hCursor, bmp.bmWidth, bmp.bmHeight)
            };
        }


        /// <summary>
        /// Obtiene el área de todas las pantallas combinadas
        /// </summary>
        /// <returns></returns>
        public static RECT GetVirtualScreenBounds()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            var screens = GetAllScreensInfo();

            foreach (var screen in screens)
            {
                minX = Math.Min(minX, screen.MonitorArea.Left);
                minY = Math.Min(minY, screen.MonitorArea.Top);
                maxX = Math.Max(maxX, screen.MonitorArea.Right);
                maxY = Math.Max(maxY, screen.MonitorArea.Bottom);
            }

            return new RECT { Left = minX, Top = minY, Right = maxX, Bottom = maxY };
        }

        /// <summary>
        /// Obtiene la pantalla que contiene unas coordenadas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static HScreenInfo? GetScreenFromPoint(int x, int y)
        {
            foreach (var screen in GetAllScreensInfo())
                if (x >= screen.MonitorArea.Left && x < screen.MonitorArea.Right && y >= screen.MonitorArea.Top && y < screen.MonitorArea.Bottom)
                    return screen;
                
            return null;
        }

        public static byte[] CaptureRegionAsBmp(RECT region, bool excludeAppWindow = false, bool includeCursor = false)
        {
            HScreenCapture capture = CaptureRegion(region, excludeAppWindow, includeCursor);
            
            if (capture.Width == 0) 
                return new byte[0];

            // Crear encabezado BMP
            int fileSize = 14 + 40 + capture.RawData.Length;
            byte[] bmpBytes = new byte[fileSize];

            // Encabezado de archivo (14 bytes)
            bmpBytes[0] = (byte)'B'; bmpBytes[1] = (byte)'M'; // Signature
            BitConverter.GetBytes(fileSize).CopyTo(bmpBytes, 2); // File size
            BitConverter.GetBytes(54).CopyTo(bmpBytes, 10); // Pixel data offset

            // Encabezado de información (40 bytes)
            BitConverter.GetBytes(40).CopyTo(bmpBytes, 14); // Header size
            BitConverter.GetBytes(capture.Width).CopyTo(bmpBytes, 18);
            BitConverter.GetBytes(capture.Height).CopyTo(bmpBytes, 22);
            BitConverter.GetBytes((short)1).CopyTo(bmpBytes, 26); // Planes
            BitConverter.GetBytes((short)32).CopyTo(bmpBytes, 28); // Bits per pixel
            BitConverter.GetBytes(0).CopyTo(bmpBytes, 30); // Compression
            BitConverter.GetBytes(capture.RawData.Length).CopyTo(bmpBytes, 34); // Image size

            // Datos de píxeles (formato BGRA)
            Array.Copy(capture.RawData, 0, bmpBytes, 54, capture.RawData.Length);

            return bmpBytes;
        }


        /// <summary>
        /// Captura todo el escritorio (todas las pantallas)
        /// </summary>
        /// <param name="excludeAppWindow">Si es true, excluye la ventana de la aplicación actual</param>
        public static HScreenCapture CaptureFullDesktop(bool excludeAppWindow = false, bool includeCursor = false)
        {
            RECT virtualBounds = GetVirtualScreenBounds();
            return CaptureRegion(virtualBounds, excludeAppWindow, includeCursor);
        }

        /// <summary>
        /// Captura todo el escritorio y lo convierte a formato BMP
        /// </summary>
        public static byte[] CaptureFullDesktopAsBmp(bool excludeAppWindow = false, bool includeCursor = false)
        {
            RECT virtualBounds = GetVirtualScreenBounds();
            return CaptureRegionAsBmp(virtualBounds, excludeAppWindow, includeCursor);
        }


        /// <summary>
        /// Captura una región específica de una pantalla determinada
        /// </summary>
        /// <param name="screenInfo">Pantalla de origen</param>
        /// <param name="region">Región relativa a la pantalla</param>
        /// <param name="excludeAppWindow">Si es true, excluye la ventana de la aplicación actual</param>
        public static HScreenCapture CaptureRegionFromScreen(HScreenInfo screenInfo, RECT region, bool excludeAppWindow = false)
        {
            // Convertir a coordenadas absolutas
            RECT absRegion = new RECT() 
            {
                Left = screenInfo.MonitorArea.Left + region.Left,
                Top = screenInfo.MonitorArea.Top + region.Top,
                Right = region.Right,
                Bottom = region.Bottom
            };

            return CaptureRegion(absRegion, excludeAppWindow);
        }

        /// <summary>
        /// Convierte una captura de pantalla a Base64 (formato BMP)
        /// </summary>
        public static string ConvertToBase64Bmp(HScreenCapture capture)
        {
            if (capture.Width == 0 || capture.Height == 0)
                return string.Empty;

            byte[] bmpBytes = ConvertToBmpBytes(
                capture.RawData,
                capture.Width,
                capture.Height,
                capture.Stride
            );

            return Convert.ToBase64String(bmpBytes);
        }

        /// <summary>
        /// Convierte una imagen de cursor a Base64 (formato BMP)
        /// </summary>
        public static string ConvertCursorToBase64Bmp(HCursorInfo cursor)
        {
            if (cursor.CursorImage == null || cursor.Size.cx == 0 || cursor.Size.cy == 0)
                return string.Empty;

            byte[] bmpBytes = ConvertToBmpBytes(
                cursor.CursorImage,
                cursor.Size.cx,
                cursor.Size.cy,
                cursor.Size.cx * 4  // Stride = width * 4 bytes (32bpp)
            );

            return Convert.ToBase64String(bmpBytes);
        }

        #region HELPERS

        /// <summary>
        /// Convierte datos de imagen a formato BMP en memoria
        /// </summary>
        private static byte[] ConvertToBmpBytes(byte[] pixelData, int width, int height, int stride)
        {
            // Calcular tamaños
            int headerSize = 54;
            int pixelDataSize = pixelData.Length;
            int fileSize = headerSize + pixelDataSize;

            // Crear buffer para BMP
            byte[] bmpBytes = new byte[fileSize];

            // Escribir cabecera BMP
            WriteBmpHeader(bmpBytes, width, height, fileSize);

            // Copiar datos de píxeles (invertir filas verticalmente)
            int rowSize = width * 4;
            for (int y = 0; y < height; y++)
            {
                int srcIndex = (height - 1 - y) * stride;
                int dstIndex = headerSize + y * rowSize;
                Buffer.BlockCopy(pixelData, srcIndex, bmpBytes, dstIndex, rowSize);
            }

            return bmpBytes;
        }

        /// <summary>
        /// Escribe la cabecera BMP en el buffer
        /// </summary>
        private static void WriteBmpHeader(byte[] buffer, int width, int height, int fileSize)
        {
            // File header (14 bytes)
            buffer[0] = (byte)'B'; buffer[1] = (byte)'M';   // Signature
            WriteInt(buffer, 2, fileSize);                  // File size
            WriteInt(buffer, 10, 54);                       // Pixel data offset

            // Info header (40 bytes)
            WriteInt(buffer, 14, 40);                       // Header size
            WriteInt(buffer, 18, width);                    // Width
            WriteInt(buffer, 22, height);                   // Height (positive = bottom-up)
            buffer[26] = 1; buffer[27] = 0;                // Planes
            buffer[28] = 32; buffer[29] = 0;               // Bits per pixel (32)
            WriteInt(buffer, 30, 0);                        // Compression (BI_RGB)
            WriteInt(buffer, 34, width * height * 4);       // Image size
            WriteInt(buffer, 38, 0);                        // X pixels per meter
            WriteInt(buffer, 42, 0);                        // Y pixels per meter
            WriteInt(buffer, 46, 0);                        // Colors used
            WriteInt(buffer, 50, 0);                        // Important colors
        }

        /// <summary>
        /// Escribe un entero de 4 bytes en little-endian
        /// </summary>
        private static void WriteInt(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        private static byte[] GetCursorImageData(nint hCursor, int width, int height)
        {
            // Crear DC y bitmap
            nint hdcScreen = Gdi32.CreateDC("DISPLAY", null, null, nint.Zero);
            nint hdcMem = Gdi32.CreateCompatibleDC(hdcScreen);
            BITMAPINFO bmi = CreateBitmapInfo(width, height);
            nint bits;
            nint hBitmap = Gdi32.CreateDIBSection(hdcScreen, ref bmi, DIB_RGB_COLORS, out bits, nint.Zero, 0);
            nint hOld = Gdi32.SelectObject(hdcMem, hBitmap);

            // Crear fondo negro para transparencias
            RECT bgRect = new RECT { Left = 0, Top = 0, Right = width, Bottom = height };
            nint hBrush = Gdi32.CreateSolidBrush(0x00000000); // Negro
            User32.FillRect(hdcMem, ref bgRect, hBrush);
            Gdi32.DeleteObject(hBrush);

            // Dibujar cursor
            User32.DrawIconEx(
                hdcMem, 0, 0, hCursor,
                0, 0, 0,
                nint.Zero,
                DI_NORMAL | DI_IMAGE | DI_MASK
            );

            // Leer datos de píxeles
            int bytes = width * height * 4;
            byte[] pixelData = new byte[bytes];
            Marshal.Copy(bits, pixelData, 0, bytes);

            // Limpiar
            Gdi32.SelectObject(hdcMem, hOld);
            Gdi32.DeleteObject(hBitmap);
            Gdi32.DeleteDC(hdcMem);
            Gdi32.DeleteDC(hdcScreen);

            return pixelData;
        }

        private static void DrawCursorOnCapture(nint hdc, int offsetX, int offsetY)
        {
            CURSORINFO cursorInfo = new CURSORINFO();
            cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

            if (!User32.GetCursorInfo(out cursorInfo)) return;

            if ((cursorInfo.flags & CURSOR_SHOWING) == CURSOR_SHOWING)
            {
                // Calcular posición relativa
                int x = cursorInfo.ptScreenPos.X - offsetX;
                int y = cursorInfo.ptScreenPos.Y - offsetY;

                // Dibujar cursor preservando transparencia
                User32.DrawIconEx(
                    hdc,
                    x,
                    y,
                    cursorInfo.hCursor,
                    0, 0, 0,
                    nint.Zero,
                    DI_NORMAL | DI_IMAGE | DI_MASK
                );
            }
        }

        private static BITMAPINFO CreateBitmapInfo(int width, int height)
        {
            BITMAPINFOHEADER header = new BITMAPINFOHEADER
            {
                biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER)),
                biWidth = width,
                biHeight = -height, // Top-down
                biPlanes = 1,
                biBitCount = 32,
                biCompression = 0, // BI_RGB
                biSizeImage = 0
            };

            return new BITMAPINFO { bmiHeader = header };
        }

        private static RECT IntersectRects(RECT a, RECT b)
        {
            return new RECT
            {
                Left = Math.Max(a.Left, b.Left),
                Top = Math.Max(a.Top, b.Top),
                Right = Math.Min(a.Right, b.Right),
                Bottom = Math.Min(a.Bottom, b.Bottom)
            };
        }

        /// <summary>
        /// Versión mejorada para exclusión de ventana con offset
        /// </summary>
        private static void ExcludeAppWindowFromCapture(nint bits, RECT captureArea, int stride, int height)
        {
            //nint appWindow = User32.GetForegroundWindow();
            nint appWindow = Handled;

            if (appWindow == nint.Zero || !User32.IsWindowVisible(appWindow))
                return;

            RECT windowRect;

            if (User32.DwmGetWindowAttribute(appWindow, DWMWA_EXTENDED_FRAME_BOUNDS, out windowRect, Marshal.SizeOf(typeof(RECT))) != 0)
                User32.GetWindowRect(appWindow, out windowRect);

            // Calcular posición relativa
            int relLeft = windowRect.Left - captureArea.Left;
            int relTop = windowRect.Top - captureArea.Top;
            int width = windowRect.Right - windowRect.Left;
            int heightWin = windowRect.Bottom - windowRect.Top;

            // Calcular área de intersección
            int startX = Math.Max(0, relLeft);
            int startY = Math.Max(0, relTop);
            int endX = Math.Min(captureArea.Width, relLeft + width);
            int endY = Math.Min(height, relTop + heightWin);

            // Rellenar con negro (formato BGRA: 0xFF000000)
            for (int y = startY; y < endY; y++)
            {
                int offset = y * stride + startX * 4;
                for (int x = startX; x < endX; x++)
                {
                    Marshal.WriteInt32(bits, offset, unchecked((int)0xFF000000));
                    offset += 4;
                }
            }
        }

        //private static unsafe void ExcludeAppWindowFromCapture(IntPtr bits, RECT captureArea, int stride, int height)
        //{
        //    nint appWindow = Handled;

        //    if (appWindow == IntPtr.Zero || !User32.IsWindowVisible(appWindow)) return;

        //    RECT windowRect;
        //    if (User32.DwmGetWindowAttribute(appWindow, DWMWA_EXTENDED_FRAME_BOUNDS, out windowRect, Marshal.SizeOf(typeof(RECT))) != 0)
        //    {
        //        User32.GetWindowRect(appWindow, out windowRect);
        //    }

        //    // Calcular posición relativa
        //    int relLeft = windowRect.Left - captureArea.Left;
        //    int relTop = windowRect.Top - captureArea.Top;
        //    int widthWin = windowRect.Right - windowRect.Left;
        //    int heightWin = windowRect.Bottom - windowRect.Top;

        //    // Calcular área de intersección
        //    int startX = Math.Max(0, relLeft);
        //    int startY = Math.Max(0, relTop);
        //    int endX = Math.Min(captureArea.Width, relLeft + widthWin);
        //    int endY = Math.Min(height, relTop + heightWin);

        //    // Crear zona transparente
        //    MakeRegionTransparent(bits, stride, startX, startY, endX, endY);
        //}

        //private static unsafe void MakeRegionTransparent(IntPtr bits, int stride, int startX, int startY, int endX, int endY)
        //{
        //    byte* pBits = (byte*)bits.ToPointer();

        //    for (int y = startY; y < endY; y++)
        //    {
        //        int rowOffset = y * stride;
        //        for (int x = startX; x < endX; x++)
        //        {
        //            int pixelOffset = rowOffset + (x * 4);

        //            // Establecer canal alpha a 0 (totalmente transparente)
        //            // Mantener los colores originales pero con alpha=0
        //            pBits[pixelOffset + 3] = 0; // Canal Alpha
        //        }
        //    }
        //}

        #endregion

    }
}