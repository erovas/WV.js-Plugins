using ScreenController.Win32.Structs;
using ScreenController.Win32.Enums;
using ScreenController.Helpers;
using ScreenController.Public;
using ScreenController.Win32;
using WV.Attributes;
using WV.Interfaces;
using WV;

namespace ScreenController
{
    [Singleton]
    public class ScreenController : Plugin
    {
        private static string[] GetStringStateFlags(StateFlags value)
        {
            List<string> listaAtributos = new List<string>();

            // Recorrer todos los valores posibles de FileAttributes
            foreach (StateFlags valor in Enum.GetValues(typeof(StateFlags)))
                // Verificar si el atributo está presente usando operaciones bitwise
                if ((value & valor) == valor && valor != 0)
                    listaAtributos.Add(valor.ToString());

            return listaAtributos.ToArray();
        }

        private static Rect RECT2Rect(RECT rect)
        {
            return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        private static ScreenInfo HScreen2Screen(HScreenInfo item)
        {
            return new ScreenInfo(
                    item.DeviceName,
                    item.DeviceID,
                    item.DeviceKey,
                    item.DeviceString,
                    RECT2Rect(item.MonitorArea),
                    RECT2Rect(item.WorkArea),
                    item.PositionX,
                    item.PositionY,
                    item.PixelWidth,
                    item.PixelHeight,
                    item.BitsPerPixel,
                    item.RefreshRate,
                    item.DpiX,
                    item.DpiY,
                    item.ScalingFactor,
                    item.IsPrimary,
                    GetStringStateFlags(item.StateFlags)
            );
        }


        public ScreenController(IWebView webView) : base(webView)
        {
            HScreenController.Handled = User32.FindWindow(this.WebView.UID, null);
        }

        #region PROPS

        public ScreenInfo[] Screens
        {
            get 
            {
                Plugin.ThrowDispose(this);
                List<ScreenInfo> screenInfos = new List<ScreenInfo>();

                foreach (var item in HScreenController.GetAllScreensInfo())
                    screenInfos.Add(HScreen2Screen(item));

                return screenInfos.ToArray();
            }
        }

        public Rect VirtualScreenBounds
        {
            get
            {
                Plugin.ThrowDispose(this);
                return RECT2Rect(HScreenController.GetVirtualScreenBounds());
            }
        }

        #endregion

        public ScreenInfo? ScreenFromPoint(int x, int y)
        {
            Plugin.ThrowDispose(this);

            foreach (var screen in HScreenController.GetAllScreensInfo())
                if (x >= screen.MonitorArea.Left && x < screen.MonitorArea.Right && y >= screen.MonitorArea.Top && y < screen.MonitorArea.Bottom)
                    return HScreen2Screen(screen);

            return null;
        }

        public string CaptureRegionBase64BMP(int x, int y, int width, int height, bool excludeAppWindow = false, bool includeCursor = false)
        {
            Plugin.ThrowDispose(this);

            RECT rect = new RECT()
            {
                Left = x,
                Top = y,
                Right = x + width,
                Bottom = y + height,
            };
            HScreenCapture region = HScreenController.CaptureRegion(rect, excludeAppWindow, includeCursor);
            return HScreenController.ConvertToBase64Bmp(region);
        }

        public byte[] CaptureRegionBytesBMP(int x, int y, int width, int height, bool excludeAppWindow = false, bool includeCursor = false)
        {
            Plugin.ThrowDispose(this);

            RECT rect = new RECT()
            {
                Left = x,
                Top = y,
                Right = x + width,
                Bottom = y + height,
            };
            return HScreenController.CaptureRegionAsBmp(rect, excludeAppWindow, includeCursor);
        }

        public string CaptureFullDesktopBase64BMP(bool excludeAppWindow = false, bool includeCursor = false)
        {
            byte[] bytes = CaptureFullDesktopBytesBMP(excludeAppWindow, includeCursor);
            return Convert.ToBase64String(bytes);
        }

        public byte[] CaptureFullDesktopBytesBMP(bool excludeAppWindow = false, bool includeCursor = false)
        {
            Plugin.ThrowDispose(this);
            return HScreenController.CaptureFullDesktopAsBmp(excludeAppWindow, includeCursor);
        }

        protected override void Dispose(bool disposing)
        {
            //throw new NotImplementedException();
        }
    }
}