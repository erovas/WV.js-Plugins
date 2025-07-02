using WV;
using WV.Attributes;
using WV.Interfaces;
using static WV.AppManager;
using MouseController.Win32;
using MouseController.Win32.Enums;
using MouseController.Win32.Structs;
using System.Runtime.InteropServices;

namespace MouseController
{
    [Singleton]
    public sealed class MouseController : Plugin
    {
        #region EVENTS

        private readonly object _eventLock = new object();
        private event WVEventHandler<int, int>? mouseMove;
        private event WVEventHandler<int, int, int>? mouseWheel;
        private event WVEventHandler<string, int, int>? mouseDown;
        private event WVEventHandler<string, int, int>? mouseUp;
        private event WVEventHandler<string, int, int>? click;
        private event WVEventHandler<string, int, int>? doubleClick;

        public event WVEventHandler<int, int> MouseMove
        {
            add { lock (_eventLock) { mouseMove += value; UpdateDeviceRegistration(); } }
            remove { lock (_eventLock) { mouseMove -= value; UpdateDeviceRegistration(); } }
        }

        public event WVEventHandler<int, int, int> MouseWheel
        {
            add { lock (_eventLock) { mouseWheel += value; UpdateDeviceRegistration(); } }
            remove { lock (_eventLock) { mouseWheel -= value; UpdateDeviceRegistration(); } }
        }

        public event WVEventHandler<string, int, int> MouseDown
        {
            add { lock (_eventLock) { mouseDown += value; UpdateDeviceRegistration(); } }
            remove { lock (_eventLock) { mouseDown -= value; UpdateDeviceRegistration(); } }
        }

        public event WVEventHandler<string, int, int> MouseUp
        {
            add { lock (_eventLock) { mouseUp += value; UpdateDeviceRegistration(); } }
            remove { lock (_eventLock) { mouseUp -= value; UpdateDeviceRegistration(); } }
        }

        public event WVEventHandler<string, int, int> Click
        {
            add { lock (_eventLock) { click += value; UpdateDeviceRegistration(); } }
            remove { lock (_eventLock) { click -= value; UpdateDeviceRegistration(); } }
        }

        public event WVEventHandler<string, int, int> DoubleClick
        {
            add { lock (_eventLock) { doubleClick += value; UpdateDeviceRegistration(); } }
            remove { lock (_eventLock) { doubleClick -= value; UpdateDeviceRegistration(); } }
        }

        #endregion

        private IntPtr Handle { get; }
        private int DoubleClickSpeed { get; }
        private DateTime LastClickTime { get; set; } = DateTime.MinValue;
        private string LastClickButton { get; set; } = string.Empty;
        private bool IsRawRegistered { get; set; } = false;
        

        public MouseController(IWebView webView) : base(webView)
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
            bool hasSubscribers = mouseMove != null || mouseDown != null ||
                                  mouseUp != null || click != null ||
                                  doubleClick != null || mouseWheel != null ||
                                  OnMouseMove != null || OnMouseDown != null ||
                                  OnMouseUp != null || OnClick != null ||
                                  OnDoubleClick != null || OnMouseWheel != null;

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
                Plugin.ThrowDispose(this);
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
                Plugin.ThrowDispose(this);

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
                Plugin.ThrowDispose(this);
                User32.GetCursorPos(out POINT point);
                return point.X;
            }
            set 
            {
                Plugin.ThrowDispose(this);
                User32.GetCursorPos(out POINT point);
                User32.SetCursorPos(value, point.Y);
            }
        }

        public int Y
        {
            get
            {
                Plugin.ThrowDispose(this);
                User32.GetCursorPos(out POINT point);
                return point.Y;
            }
            set
            {
                Plugin.ThrowDispose(this);
                User32.GetCursorPos(out POINT point);
                User32.SetCursorPos(point.X, value);
            }
        }

        public bool IsCursorClipped { get; private set; }

        #endregion

        #region METHODS

        public void ClipCursor(int x, int y, int width, int height)
        {
            Plugin.ThrowDispose(this);

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

        public void UnclipCursor()
        {
            Plugin.ThrowDispose(this);

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
            Plugin.ThrowDispose(this);
            User32.SetCursorPos(x, y);
        }

        /// <summary>
        /// Obtiene la posición actual del cursor
        /// </summary>
        public int[] GetPosition()
        {
            Plugin.ThrowDispose(this);
            User32.GetCursorPos(out POINT point);
            return [point.X, point.Y];
        }

        /// <summary>
        /// Hacer click
        /// </summary>
        /// <param name="button"></param>
        public void DoClick(string button)
        {
            Plugin.ThrowDispose(this);
            (MouseEventFlags, uint) btn = GetClickEnum(GetButtonName(button));
            User32.mouse_event((uint)btn.Item1, 0, 0, btn.Item2, 0);
        }

        /// <summary>
        /// Doble click
        /// </summary>
        /// <param name="button"></param>
        public void DoDoubleClick(string button)
        {
            Plugin.ThrowDispose(this);
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
            Plugin.ThrowDispose(this);
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
            Plugin.ThrowDispose(this);
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
            Plugin.ThrowDispose(this);
            User32.mouse_event((uint)MouseEventFlags.WHEEL, 0, 0, (uint)delta, 0);
        }

        /// <summary>
        /// Scroll horizontal
        /// </summary>
        /// <param name="delta">Valor positivo para derecha, negativo para izquierda</param>
        public void HorizontalScroll(int delta)
        {
            Plugin.ThrowDispose(this);
            User32.mouse_event((uint)MouseEventFlags.HWHEEL, 0, 0, (uint)delta, 0);
        }

        /// <summary>
        /// Doble click con intervalo personalizable
        /// </summary>
        public void CustomDoubleClick(string button, int intervalMs)
        {
            Plugin.ThrowDispose(this);
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
            Plugin.ThrowDispose(this);
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
            Plugin.ThrowDispose(this);
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


        #endregion

        #region EVENTS

        #region MouseMove

        private IJSFunction? OnMouseMoveFN { get; set; }

        public object? OnMouseMove
        {
            get 
            {
                Plugin.ThrowDispose(this);
                return this.OnMouseMoveFN != null ? this.OnMouseMoveFN.Raw : null;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if(this.OnMouseMoveFN != null && value == this.OnMouseMoveFN.Raw)
                    return;

                this.OnMouseMoveFN?.Dispose();
                this.OnMouseMoveFN = null;

                if (value != null)
                    this.OnMouseMoveFN = IJSFunction.Create(value);

                this.UpdateDeviceRegistration();
            }
        }

        private void FireMouseMove(int x, int y)
        {
            this.OnMouseMoveFN?.Execute(x, y);
            this.mouseMove?.Invoke(this.WebView, x, y);
        }

        #endregion

        #region MouseWheel

        private IJSFunction? OnMouseWheelFN { get; set; }

        public object? OnMouseWheel
        {
            get
            {
                Plugin.ThrowDispose(this);
                return this.OnMouseWheelFN?.Raw;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (this.OnMouseWheelFN != null && value == this.OnMouseWheelFN.Raw)
                    return;

                this.OnMouseWheelFN?.Dispose();
                this.OnMouseWheelFN = null;

                if (value != null)
                    this.OnMouseWheelFN = IJSFunction.Create(value);

                this.UpdateDeviceRegistration();
            }
        }

        private void FireMouseWheel(int delta, int x, int y)
        {
            this.OnMouseWheelFN?.Execute(x, y);
            this.mouseWheel?.Invoke(this.WebView, delta, x, y);
        }

        #endregion

        #region MouseDown

        private IJSFunction? OnMouseDownFN { get; set; }

        public object? OnMouseDown
        {
            get
            {
                Plugin.ThrowDispose(this);
                return this.OnMouseDownFN?.Raw;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (this.OnMouseDownFN != null && value == this.OnMouseDownFN.Raw)
                    return;

                this.OnMouseDownFN?.Dispose();
                this.OnMouseDownFN = null;

                if (value != null)
                    this.OnMouseDownFN = IJSFunction.Create(value);

                this.UpdateDeviceRegistration();
            }
        }

        private void FireMouseDown(string button, int x, int y)
        {
            this.OnMouseDownFN?.Execute(x, y);
            this.mouseDown?.Invoke(this.WebView, button, x, y);
        }

        #endregion

        #region MouseUp

        private IJSFunction? OnMouseUpFN { get; set; }

        public object? OnMouseUp
        {
            get
            {
                Plugin.ThrowDispose(this);
                return this.OnMouseUpFN?.Raw;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (this.OnMouseUpFN != null && value == this.OnMouseUpFN.Raw)
                    return;

                this.OnMouseUpFN?.Dispose();
                this.OnMouseUpFN = null;

                if (value != null)
                    this.OnMouseUpFN = IJSFunction.Create(value);

                this.UpdateDeviceRegistration();
            }
        }

        private void FireMouseUp(string button, int x, int y)
        {
            this.OnMouseUpFN?.Execute(x, y);
            this.mouseUp?.Invoke(this.WebView, button, x, y);
        }

        #endregion

        #region Click

        private IJSFunction? OnClickFN { get; set; }

        public object? OnClick
        {
            get
            {
                Plugin.ThrowDispose(this);
                return this.OnClickFN?.Raw;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (this.OnClickFN != null && value == this.OnClickFN.Raw)
                    return;

                this.OnClickFN?.Dispose();
                this.OnClickFN = null;

                if (value != null)
                    this.OnClickFN = IJSFunction.Create(value);

                this.UpdateDeviceRegistration();
            }
        }

        private void FireClick(string button, int x, int y)
        {
            this.OnClickFN?.Execute(button, x, y);
            this.click?.Invoke(this.WebView, button, x, y);
        }

        #endregion

        #region DoubleClick

        private IJSFunction? OnDoubleClickFN { get; set; }

        public object? OnDoubleClick
        {
            get
            {
                Plugin.ThrowDispose(this);
                return this.OnDoubleClickFN?.Raw;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (this.OnDoubleClickFN != null && value == this.OnDoubleClickFN.Raw)
                    return;

                this.OnDoubleClickFN?.Dispose();
                this.OnDoubleClickFN = null;

                if (value != null)
                    this.OnDoubleClickFN = IJSFunction.Create(value);

                this.UpdateDeviceRegistration();
            }
        }

        private void FireDoubleClick(string button, int x, int y)
        {
            this.OnDoubleClickFN?.Execute(button, x, y);
            this.doubleClick?.Invoke(this.WebView, button, x, y);
        }

        #endregion

        #endregion

        protected override void Dispose(bool disposing)
        {
            if(this.Disposed)
                return;

            this.WebView.Window.Raw -= Window_Raw;
            this.ClearOnEvents();
            this.UpdateDeviceRegistration();
        }

        private void ClearOnEvents()
        {
            this.OnMouseMove = null;
            this.OnMouseWheel = null;
            this.OnMouseDown = null;
            this.OnMouseUp = null;
            this.OnClickFN = null;
            this.OnDoubleClickFN = null;
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