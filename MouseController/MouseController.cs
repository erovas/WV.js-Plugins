using WV;
using WV.Interfaces;
using MouseController.Win32;
using static WV.App.Delegates;
using MouseController.Win32.Enums;
using MouseController.Win32.Structs;
using System.Runtime.InteropServices;

namespace MouseController
{
    public sealed class MouseController : Plugin
    {
        #region EVENTS

        public event WVEventHandler<int, int>? MouseMove;

        public event WVEventHandler<int, int, int>? MouseWheel;

        public event WVEventHandler<string, int, int>? MouseDown;

        public event WVEventHandler<string, int, int>? MouseUp;

        public event WVEventHandler<string, int, int>? Click;

        public event WVEventHandler<string, int, int>? DoubleClick;

        #endregion

        private IntPtr Handle { get; }
        private int DoubleClickSpeed { get; }
        private DateTime LastClickTime { get; set; } = DateTime.MinValue;
        private string LastClickButton { get; set; } = string.Empty;
        private bool IsRawRegistered { get; set; } = false;
        

        public MouseController(IPluginContext ctx) : base(ctx)
        {
            this.Handle = User32.FindWindow(this.WebView.UID, null);
            this.DoubleClickSpeed = User32.GetDoubleClickTime();

            this.WebView.Window.Raw += Window_Raw;
        }

        private void Window_Raw(IWebView sender, object[] args, ref bool handled)
        {
            if (this.Disposed)
                return;

            //IntPtr hWnd = (IntPtr)args[0];
            uint uMsg = (uint)args[1];
            //IntPtr wParam = (IntPtr)args[2];
            IntPtr lParam = (IntPtr)args[3];

            if (uMsg == 0x00FF && this.IsRawRegistered) //WM_INPUT
            {
                uint size = 0;
                User32.GetRawInputData(lParam, 0x10000003, IntPtr.Zero, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                try
                {
                    User32.GetRawInputData(lParam, 0x10000003, buffer, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));
                    var rawInput = Marshal.PtrToStructure<RAWINPUTHEADER>(buffer);

                    if (rawInput.dwType == 0) // RIM_TYPEMOUSE
                    {
                        var mouse = Marshal.PtrToStructure<RAWMOUSE>(buffer + Marshal.SizeOf(typeof(RAWINPUTHEADER)));
                        ProcessMouseData(mouse);
                        handled = true;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
        
        private void ProcessMouseData(RAWMOUSE mouse)
        {
            // Obtener coordenadas del cursor
            User32.GetCursorPos(out POINT pt);
            int x = pt.X;
            int y = pt.Y;

            // Manejar movimiento
            this.FireMouseMove(x, y);

            // Manejar botones
            HandleMouseButtons(mouse.usButtonFlags, x, y);

            // Manejar Wheel
            if ((mouse.usButtonFlags & 0x0400) != 0) // RI_MOUSE_WHEEL
                this.FireMouseWheel((short)mouse.usButtonData, x, y);
        }

        private void HandleMouseButtons(uint buttons, int x, int y)
        {
            // Flags de botones: https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawmouse
            var downFlags = new[]
            {
                (0x0001, "LEFT"),    // RI_MOUSE_LEFT_BUTTON_DOWN
                (0x0004, "RIGHT"),   // RI_MOUSE_RIGHT_BUTTON_DOWN
                (0x0010, "MIDDLE"),  // RI_MOUSE_MIDDLE_BUTTON_DOWN
                (0x0040, "X1"),      // RI_MOUSE_BUTTON_4_DOWN
                (0x0100, "X2")       // RI_MOUSE_BUTTON_5_DOWN
            };

            var upFlags = new[]
            {
                (0x0002, "LEFT"),    // RI_MOUSE_LEFT_BUTTON_UP
                (0x0008, "RIGHT"),   // RI_MOUSE_RIGHT_BUTTON_UP
                (0x0020, "MIDDLE"),  // RI_MOUSE_MIDDLE_BUTTON_UP
                (0x0080, "X1"),      // RI_MOUSE_BUTTON_4_UP
                (0x0200, "X2")       // RI_MOUSE_BUTTON_5_UP
            };

            foreach (var (flag, button) in downFlags)
            {
                if ((buttons & flag) != 0)
                {
                    this.FireMouseDown(button, x, y);
                    break;
                }
            }

            foreach (var (flag, button) in upFlags)
            {
                if ((buttons & flag) != 0)
                {
                    this.FireMouseUp(button, x, y);
                    CheckForClick(button, x, y);
                    break;
                }
            }
        }

        private void CheckForClick(string button, int x, int y)
        {
            var now = DateTime.Now;
            var elapsed = (now - LastClickTime).TotalMilliseconds;

            if (elapsed <= DoubleClickSpeed && LastClickButton == button)
            {
                
                this.FireDoubleClick(button, x, y);
                LastClickTime = DateTime.MinValue;
            }
            else
            {
                this.FireClick(button, x, y);
                LastClickTime = now;
                LastClickButton = button;
            }
        }

        private void UpdateDeviceRegistration()
        {
            bool hasSubscribers = MouseMove != null || MouseDown != null ||
                                  MouseUp != null || Click != null ||
                                  DoubleClick != null || MouseWheel != null;

            if (hasSubscribers && !this.IsRawRegistered)
            {
                RegisterRawInput(this.Handle, true);
                this.IsRawRegistered = true;
            }
            else if (!hasSubscribers && this.IsRawRegistered)
            {
                RegisterRawInput(this.Handle, false);
                this.IsRawRegistered = false;
            }
        }

        #region PROPS

        /// <summary>
        /// Speed is an integer value between 0 and 20. 10 is the default.
        /// </summary>
        public int Speed
        { 
            get
            {
                ThrowIfDisposed();
                int intSpeed = 0;
                IntPtr ptr;
                ptr = Marshal.AllocCoTaskMem(4);
                User32.SystemParametersInfo((int)SystemParameters.SPI_GETMOUSESPEED, 0, ptr, 0);
                intSpeed = Marshal.ReadInt32(ptr);
                Marshal.FreeCoTaskMem(ptr);
                return intSpeed;
            }
            set
            {
                ThrowIfDisposed();

                if(value < 0)
                    value = 0;
                else if(value > 20)
                    value = 20;

                IntPtr ptr = new IntPtr(value);
                bool setted = User32.SystemParametersInfo((int)SystemParameters.SPI_SETMOUSESPEED, 0, ptr, 0);

                if (!setted)
                    throw new Exception("Not able to set mouse speed");
            }
        }

        public int X
        {
            get 
            { 
                ThrowIfDisposed();
                User32.GetCursorPos(out POINT point);
                return point.X;
            }
            set 
            {
                ThrowIfDisposed();
                User32.GetCursorPos(out POINT point);
                User32.SetCursorPos(value, point.Y);
            }
        }

        public int Y
        {
            get
            {
                ThrowIfDisposed();
                User32.GetCursorPos(out POINT point);
                return point.Y;
            }
            set
            {
                ThrowIfDisposed();
                User32.GetCursorPos(out POINT point);
                User32.SetCursorPos(point.X, value);
            }
        }

        public bool IsCursorClipped { get; private set; }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ClipCursor(int x, int y, int width, int height)
        {
            ThrowIfDisposed();

            if(this.IsCursorClipped)
                return;

            RECT restrictionArea = new RECT
            {
                Left = x,   // X inicial
                Top = y,    // Y inicial
                Right = width,  // X final
                Bottom = height  // Y final
            };

            if(User32.ClipCursor(ref restrictionArea))
                this.IsCursorClipped = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnclipCursor()
        {
            ThrowIfDisposed();

            if (!this.IsCursorClipped)
                return;

            if (User32.ClipCursor(IntPtr.Zero))
                this.IsCursorClipped = false;
        }

        /// <summary>
        /// Mueve el cursor a una posición específica usando Point
        /// </summary>
        public void SetPosition(int x, int y)
        {
            ThrowIfDisposed();
            User32.SetCursorPos(x, y);
        }

        /// <summary>
        /// Obtiene la posición actual del cursor
        /// </summary>
        public int[] GetPosition()
        {
            ThrowIfDisposed();
            User32.GetCursorPos(out POINT point);
            return [point.X, point.Y];
        }

        /// <summary>
        /// Hacer click
        /// </summary>
        /// <param name="button"></param>
        public void DoClick(string button)
        {
            ThrowIfDisposed();
            (MouseEventFlags, uint) btn = GetClickEnum(GetButtonName(button));
            User32.mouse_event((uint)btn.Item1, 0, 0, btn.Item2, 0);
        }

        /// <summary>
        /// Doble click
        /// </summary>
        /// <param name="button"></param>
        public void DoDoubleClick(string button)
        {
            ThrowIfDisposed();
            (MouseEventFlags, uint) btn = GetClickEnum(GetButtonName(button));
            User32.mouse_event((uint)btn.Item1, 0, 0, btn.Item2, 0);
            User32.mouse_event((uint)btn.Item1, 0, 0, btn.Item2, 0);
        }

        /// <summary>
        /// Mantener presionado botón
        /// </summary>
        /// <param name="button"></param>
        public void ButtonDown(string button)
        {
            ThrowIfDisposed();
            MouseEventFlags btn = MouseEventFlags.X_DOWN;
            uint dwdata = 0;

            switch (GetButtonName(button))
            {
                case "LEFT":
                    btn = MouseEventFlags.LEFT_DOWN;
                    break;

                case "RIGHT":
                    btn = MouseEventFlags.RIGHT_DOWN;
                    break;

                case "MIDDLE":
                    btn = MouseEventFlags.MIDDLE_DOWN;
                    break;

                case "X1":
                    dwdata = 0x0001;
                    break;

                default:
                    dwdata = 0x0002;
                    break;
            }

            User32.mouse_event((uint)btn, 0, 0, dwdata, 0);
        }

        /// <summary>
        /// Liberar botón
        /// </summary>
        /// <param name="button"></param>
        public void ButtonUp(string button)
        {
            ThrowIfDisposed();
            MouseEventFlags btn = MouseEventFlags.X_UP;
            uint dwdata = 0;

            switch (GetButtonName(button))
            {
                case "LEFT":
                    btn = MouseEventFlags.LEFT_UP;
                    break;

                case "RIGHT":
                    btn = MouseEventFlags.RIGHT_UP;
                    break;

                case "MIDDLE":
                    btn = MouseEventFlags.MIDDLE_UP;
                    break;

                case "X1":
                    dwdata = 0x0001;
                    break;

                default:
                    dwdata = 0x0002;
                    break;
            }

            User32.mouse_event((uint)btn, 0, 0, dwdata, 0);
        }

        /// <summary>
        /// Scroll vertical
        /// </summary>
        /// <param name="delta">Valor positivo para arriba, negativo para abajo</param>
        public void VerticalScroll(int delta)
        {
            ThrowIfDisposed();
            User32.mouse_event((uint)MouseEventFlags.WHEEL, 0, 0, (uint)delta, 0);
        }

        /// <summary>
        /// Scroll horizontal
        /// </summary>
        /// <param name="delta">Valor positivo para derecha, negativo para izquierda</param>
        public void HorizontalScroll(int delta)
        {
            ThrowIfDisposed();
            User32.mouse_event((uint)MouseEventFlags.HWHEEL, 0, 0, (uint)delta, 0);
        }

        /// <summary>
        /// Doble click con intervalo personalizable
        /// </summary>
        public void CustomDoubleClick(string button, int intervalMs)
        {
            ThrowIfDisposed();
            (MouseEventFlags, uint) btn = GetClickEnum(GetButtonName(button));
            User32.mouse_event((uint)btn.Item1, 0, 0, btn.Item2, 0);
            System.Threading.Thread.Sleep(intervalMs);
            User32.mouse_event((uint)btn.Item1, 0, 0, btn.Item2, 0);
        }

        /// <summary>
        /// Scroll acelerado vertical (simula scroll humano)
        /// </summary>
        public void AcceleratedVerticalScroll(int totalSteps, int initialDelta = 50)
        {
            int delta = initialDelta;
            for (int i = 0; i < totalSteps; i++)
            {
                User32.mouse_event((uint)MouseEventFlags.WHEEL, 0, 0, (uint)delta, 0);
                delta = (int)(delta * 0.8);
                System.Threading.Thread.Sleep(30);
            }
        }

        /// <summary>
        /// Scroll acelerado horizontal (simula scroll humano)
        /// </summary>
        public void AcceleratedHorizontalScroll(int totalSteps, int initialDelta = 50)
        {
            ThrowIfDisposed();
            int delta = initialDelta;
            for (int i = 0; i < totalSteps; i++)
            {
                User32.mouse_event((uint)MouseEventFlags.HWHEEL, 0, 0, (uint)delta, 0);
                delta = (int)(delta * 0.8);
                System.Threading.Thread.Sleep(30);
            }
        }

        /// <summary>
        /// Movimiento suavizado a posición
        /// </summary>
        public void SmoothMove(int pX, int pY, int durationMs)
        {
            ThrowIfDisposed();
            User32.GetCursorPos(out POINT start);
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            while (timer.ElapsedMilliseconds < durationMs)
            {
                float progress = timer.ElapsedMilliseconds / (float)durationMs;
                int x = (int)(start.X + (pX - start.X) * progress);
                int y = (int)(start.Y + (pY - start.Y) * progress);

                User32.SetCursorPos(x, y);
                System.Threading.Thread.Sleep(10);
            }
            User32.SetCursorPos(pX, pY);
        }

        public override void AddEventListener(string type, object callback)
        {
            try
            {
                base.AddEventListener(type, callback);
                UpdateDeviceRegistration();
            }
            catch (Exception ex)
            {
                throw (ex.InnerException ?? ex);
            }
        }

        public override void RemoveEventListener(string type, object callback)
        {
            try
            {
                base.RemoveEventListener(type, callback);
                UpdateDeviceRegistration();
            }
            catch (Exception ex)
            {
                throw (ex.InnerException ?? ex);
            }
        }

        #endregion

        #region Fire Events

        private void FireMouseMove(int x, int y)
        {
            this.MouseMove?.Invoke(this.WebView, x, y);
        }

        private void FireMouseWheel(int delta, int x, int y)
        {
            this.MouseWheel?.Invoke(this.WebView, delta, x, y);
        }

        private void FireMouseDown(string button, int x, int y)
        {
            this.MouseDown?.Invoke(this.WebView, button, x, y);
        }

        private void FireMouseUp(string button, int x, int y)
        {
            this.MouseUp?.Invoke(this.WebView, button, x, y);
        }

        private void FireClick(string button, int x, int y)
        {
            this.Click?.Invoke(this.WebView, button, x, y);
        }

        private void FireDoubleClick(string button, int x, int y)
        {
            this.DoubleClick?.Invoke(this.WebView, button, x, y);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if(this.Disposed)
                return;

            this.WebView.Window.Raw -= Window_Raw;
            this.UpdateDeviceRegistration();
        }

        #region HELPERs

        private static readonly string[] BUTTONS = new string[] { "LEFT", "MIDDLE", "RIGHT", "X1", "X2" };

        private static string GetButtonName(string button)
        {
            if (string.IsNullOrWhiteSpace(button))
                throw new Exception("Invalid button");

            button = button.ToUpper().Trim();

            foreach (string item in BUTTONS)
                if (item == button)
                    return item;

            throw new Exception("Invalid button");
        }

        private static void RegisterRawInput(IntPtr hwnd, bool register)
        {
            var devices = new RAWINPUTDEVICE[1];
            uint flag = register ? 0x00000100 /* RIDEV_INPUTSINK */ : (uint)0x00000001 /* RIDEV_REMOVE */;

            devices[0] = new RAWINPUTDEVICE
            {
                usUsagePage = 0x01, // HID_USAGE_PAGE_GENERIC
                usUsage = 0x02,     // HID_USAGE_GENERIC_MOUSE
                dwFlags = flag,
                hwndTarget = register ? hwnd : IntPtr.Zero
            };

            if (!User32.RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        private static (MouseEventFlags, uint) GetClickEnum(string button)
        {
            MouseEventFlags btn = MouseEventFlags.X_DOWN | MouseEventFlags.X_UP;
            uint dwdata = 0;

            switch (button.ToUpper())
            {
                case "LEFT":
                    btn = MouseEventFlags.LEFT_DOWN | MouseEventFlags.LEFT_UP;
                    break;

                case "RIGHT":
                    btn = MouseEventFlags.RIGHT_DOWN | MouseEventFlags.RIGHT_UP;
                    break;

                case "MIDDLE":
                    btn = MouseEventFlags.MIDDLE_DOWN | MouseEventFlags.MIDDLE_UP;
                    break;

                case "X1":
                    dwdata = 0x0001;
                    break;

                default:
                    dwdata = 0x0002;
                    break;
            }

            return (btn, dwdata);
        }


        #endregion
    }
}