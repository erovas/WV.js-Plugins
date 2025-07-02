using WV;
using WV.Interfaces;
using NotifyIcon.Win32;
using static WV.AppManager;
using System.ComponentModel;
using NotifyIcon.Win32.Enums;
using NotifyIcon.Win32.Structs;
using System.Runtime.InteropServices;

namespace NotifyIcon
{
    public sealed class TrayIcon : Plugin
    {
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        private static string GetXbutton(IntPtr ptr)
        {
            int val32 = (ptr.ToInt32() >> 16) & 0xffff;
            return val32 == 0x0001 ? "X1" : "X2";   // 0x0002
        }

        #region EVENTS

        public event WVEventHandler<string, int, int>? MouseDown;
        public event WVEventHandler<string, int, int>? MouseUp;
        public event WVEventHandler<string, int, int>? Click;
        public event WVEventHandler<string, int, int>? DoubleClick;

        #endregion


        private IntPtr WinHandle { get; }

        private NOTIFYICONDATA TIHandle;

        private static int _CallbackMessageID = 0x0400 + 1;
        private static int CallbackMessageID => ++_CallbackMessageID;

        #region CONSTRUCTORS

        private int DoubleClickSpeed { get; } = 500;

        public TrayIcon(IWebView webView) : this(webView, "")
        {
        }

        public TrayIcon(IWebView webView, string icon) : this(webView, icon, "")
        {
        }

        public TrayIcon(IWebView webView, string icon, string tooltip) : base(webView)
        {
            // Configurar el icono
            this.WinHandle = User32.FindWindow(this.WebView.UID, null);
            this.IconHandle = Helper.GetDefaultIconHandle();
            this.DoubleClickSpeed = User32.GetDoubleClickTime();

            this.TIHandle = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                hWnd = this.WinHandle,
                uID = 0,
                uFlags = NotifyIconFlag.NIF_MESSAGE | NotifyIconFlag.NIF_ICON | NotifyIconFlag.NIF_TIP | NotifyIconFlag.NIF_INFO,
                uCallbackMessage = CallbackMessageID,
                hIcon = IconHandle,
                szTip = string.Empty,
                guidItem = Guid.NewGuid(),
                uTimeoutOrVersion = 4
            };

            this.WebView.Window.Raw += Window_Raw;

            this.Icon = icon;
            this.ToolTip = tooltip;
        }

        #endregion

        #region PROPS

        #region ToolTip

        private bool TooltipModified { get; set; }
        private string _ToolTip = string.Empty;
        public string ToolTip
        {
            get 
            {
                Plugin.ThrowDispose(this);
                return _ToolTip;
            } 
            set
            {
                Plugin.ThrowDispose(this);

                // Limitar a 127 caracteres (tamaño del buffer en NOTIFYICONDATA)
                value = value.Length > 127 ? value.Substring(0, 127) : value;
                this.TIHandle.szTip = value;
                this.TIHandle.uFlags = NotifyIconFlag.NIF_TIP;
                this.TooltipModified = true;

                if (this.Visible)
                {
                    this.TIHandle.uFlags = NotifyIconFlag.NIF_TIP;

                    if (!Shell32.Shell_NotifyIcon(NotifyIconMessage.NIM_MODIFY, ref this.TIHandle))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    this.TooltipModified = false;
                }
                    
                _ToolTip = value;
            }
        }

        #endregion

        #region Icon

        private IntPtr IconHandle { get; set; }
        private bool IconModified { get; set; }

        private string _Icon = string.Empty;
        public string Icon
        {
            get
            {
                Plugin.ThrowDispose(this);
                return _Icon;
            }
            set
            {
                Plugin.ThrowDispose(this);

                IntPtr newHandle;
                string fullPath = value;
                
                if (!string.IsNullOrEmpty(value))
                {
                    if (!Path.IsPathFullyQualified(value))
                        fullPath = AppManager.SrcPath + "/" + value;

                    if (!File.Exists(fullPath))
                        throw new FileNotFoundException("File not found: '" + value + "'");

                    // Cargar icono
                    newHandle = User32.LoadImage(
                        IntPtr.Zero,
                        fullPath,
                        1, // IMAGE_ICON
                        16, //width,
                        16, //height,
                        0x00000010 // LR_LOADFROMFILE
                    );
                }
                else
                    newHandle = Helper.GetDefaultIconHandle(); // Icono de aplicación por defecto

                if (newHandle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // Destruir icono anterior
                if (this.IconHandle != IntPtr.Zero)
                    User32.DestroyIcon(this.IconHandle);

                // Actualizar estructura
                this.IconHandle = newHandle;
                TIHandle.hIcon = newHandle;

                if (this.Visible)
                {
                    TIHandle.uFlags = NotifyIconFlag.NIF_ICON;

                    if (!Shell32.Shell_NotifyIcon(NotifyIconMessage.NIM_MODIFY, ref TIHandle))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    this.IconModified = false;
                }
                else
                    IconModified = true;

                _Icon = value;
            }
        }
        
        #endregion

        #region Visible

        private bool _Visible;
        public bool Visible
        {
            get
            {
                Plugin.ThrowDispose(this);
                return _Visible;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (_Visible == value)
                    return;

                if (value)
                {
                    this.TIHandle.uFlags = NotifyIconFlag.NIF_MESSAGE | NotifyIconFlag.NIF_ICON | NotifyIconFlag.NIF_TIP | NotifyIconFlag.NIF_INFO;

                    if (this.IconModified)
                    {
                        this.TIHandle.uFlags |= NotifyIconFlag.NIF_ICON;
                        this.IconModified = false;
                    }

                    if (this.TooltipModified)
                    {
                        this.TIHandle.uFlags |= NotifyIconFlag.NIF_TIP;
                        this.TooltipModified = false;
                    }
                    
                }
                    
                else
                    this.TIHandle.uFlags = 0;

                if (!Shell32.Shell_NotifyIcon(value? NotifyIconMessage.NIM_ADD : NotifyIconMessage.NIM_DELETE, ref this.TIHandle))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                if (value && !Shell32.Shell_NotifyIcon(NotifyIconMessage.NIM_SETVERSION, ref this.TIHandle))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                _Visible = value;
            }
        }

        #endregion

        #endregion

        #region METHODS

        public void Show()
        {
            Plugin.ThrowDispose(this);
            this.Visible = true;
        }

        public void Hide()
        {
            Plugin.ThrowDispose(this);
            this.Visible = false;
        }

        #region ShowNotification

        public void ShowNotification(string title, string message, string iconSource) // Puede ser: "system", "warning", "error", o ruta de archivo
        {
            IntPtr hIcon = Helper.GetNotificationIcon(iconSource);

            this.TIHandle.uFlags = NotifyIconFlag.NIF_INFO; // | NotifyIconFlag.NIF_ICON | NotifyIconFlag.NIF_MESSAGE;
            this.TIHandle.szInfoTitle = title;
            this.TIHandle.szInfo = message;
            this.TIHandle.dwInfoFlags = Helper.GetIconFlags(iconSource);
            this.TIHandle.hBalloonIcon = hIcon;
            //this.NIHandle.guidItem = Guid.NewGuid();
            //this.NIHandle.uTimeoutOrVersion = 4;
            //this.NIHandle.uCallbackMessage = WM_USER + 2;

            if (!Shell32.Shell_NotifyIcon(NotifyIconMessage.NIM_MODIFY, ref this.TIHandle))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        #endregion

        #endregion

        private string lastButtonDown = string.Empty;
        private string currentButtonUp = string.Empty;
        private long lastDownTime;
        private long currentUpTime;

        private bool IsClick => lastButtonDown == currentButtonUp && currentUpTime - lastDownTime <= this.DoubleClickSpeed;

        private void Window_Raw(IWebView sender, object[] args, ref bool handled)
        {
            if (this.Disposed)
                return;

            IntPtr hWnd = (IntPtr)args[0];
            uint uMsg = (uint)args[1];
            IntPtr wParam = (IntPtr)args[2];
            IntPtr lParam = (IntPtr)args[3];

            if (uMsg != this.TIHandle.uCallbackMessage)
                return;

            handled = true;

            switch ((WinMsg)((uint)lParam))
            {
                case WinMsg.WM_LBUTTONDOWN:
                    lastButtonDown = "LEFT";
                    lastDownTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this.FireMouseDown(lastButtonDown, Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_MBUTTONDOWN:
                    lastButtonDown = "MIDDLE";
                    lastDownTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this.FireMouseDown(lastButtonDown, Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_RBUTTONDOWN:
                    lastButtonDown = "RIGHT";
                    lastDownTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this.FireMouseDown(lastButtonDown, Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_XBUTTONDOWN:
                    lastButtonDown = GetXbutton(wParam);
                    lastDownTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this.FireMouseDown(lastButtonDown, Helper.GetCursorPosition());
                    break;

                //----------------------------//

                case WinMsg.WM_LBUTTONUP:
                    currentButtonUp = "LEFT";
                    this.FireMouseUp(currentButtonUp, Helper.GetCursorPosition());
                    currentUpTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this.FireClick(lastButtonDown, Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_MBUTTONUP:
                    currentButtonUp = "MIDDLE";
                    currentUpTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this.FireMouseUp(currentButtonUp, Helper.GetCursorPosition());
                    this.FireClick(lastButtonDown, Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_RBUTTONUP:
                    currentButtonUp = "RIGHT";
                    currentUpTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this.FireMouseUp(currentButtonUp, Helper.GetCursorPosition());
                    this.FireClick(lastButtonDown, Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_XBUTTONUP:
                    currentButtonUp = GetXbutton(wParam);
                    currentUpTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this.FireMouseUp(GetXbutton(wParam), Helper.GetCursorPosition());
                    this.FireClick(lastButtonDown, Helper.GetCursorPosition());
                    break;

                //----------------------------//

                case WinMsg.WM_LBUTTONDBLCLK:
                    this.FireDoubleClick("LEFT", Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_MBUTTONDBLCLK:
                    this.FireDoubleClick("MIDDLE", Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_RBUTTONDBLCLK:
                    this.FireDoubleClick("RIGHT", Helper.GetCursorPosition());
                    break;

                case WinMsg.WM_XBUTTONDBLCLK:
                    this.FireDoubleClick(GetXbutton(wParam), Helper.GetCursorPosition());
                    break;

                default:
                    handled = false;
                    break;
            }
        }

        #region EVENTS

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

                if (value == null)
                    return;

                this.OnMouseDownFN = IJSFunction.Create(value);
            }
        }

        private void FireMouseDown(string button, POINT pos)
        {
            this.OnMouseDownFN?.Execute(button, pos.X, pos.Y);
            this.MouseDown?.Invoke(this.WebView, button, pos.X, pos.Y);
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

                if (value == null)
                    return;

                this.OnMouseUpFN = IJSFunction.Create(value);
            }
        }

        private void FireMouseUp(string button,POINT pos)
        {
            this.OnMouseUpFN?.Execute(button, pos.X, pos.Y);
            this.MouseUp?.Invoke(this.WebView, button, pos.X, pos.Y);
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

                if (value == null)
                    return;

                this.OnClickFN = IJSFunction.Create(value);
            }
        }

        private void FireClick(string button, POINT pos)
        {
            if (!this.IsClick)
                return;

            this.OnClickFN?.Execute(button, pos.X, pos.Y);
            this.Click?.Invoke(this.WebView, button, pos.X, pos.Y);
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

                if (value == null)
                    return;

                this.OnDoubleClickFN = IJSFunction.Create(value);
            }
        }

        private void FireDoubleClick(string button, POINT pos)
        {
            this.OnDoubleClickFN?.Execute(button, pos.X, pos.Y);
            this.DoubleClick?.Invoke(this.WebView, button, pos.X, pos.Y);
        }

        #endregion

        #endregion

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.WebView.Window.Raw -= Window_Raw;

                // limpiar los eventos JS
                this.ClearOnEvents();

                // Eliminar icono de la bandeja
                if (this.Visible)
                {
                    this.TIHandle.uFlags = 0;
                    Shell32.Shell_NotifyIcon(NotifyIconMessage.NIM_DELETE, ref this.TIHandle);
                }

                // Destruir referencia del Icono
                if (this.IconHandle != IntPtr.Zero)
                {
                    User32.DestroyIcon(this.IconHandle);
                    this.IconHandle = IntPtr.Zero;
                }
            }
            catch (Exception){ }
        }

        private void ClearOnEvents()
        {
            this.OnMouseDown = null;
            this.OnMouseUp = null;
            this.OnClickFN = null;
            this.OnDoubleClickFN = null;
        }

    }
}