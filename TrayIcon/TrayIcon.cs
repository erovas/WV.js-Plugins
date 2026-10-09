using WV;
using WV.Interfaces;
using NotifyIcon.Win32;
using System.ComponentModel;
using NotifyIcon.Win32.Enums;
using static WV.App.Delegates;
using NotifyIcon.Win32.Structs;
using System.Runtime.InteropServices;

namespace NotifyIcon
{
    public sealed class TrayIcon : Plugin
    {
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

        private readonly string Directory;

        public TrayIcon(IPluginContext ctx) : this(ctx, "")
        {
        }

        public TrayIcon(IPluginContext ctx, string icon) : this(ctx, icon, "")
        {
        }

        public TrayIcon(IPluginContext ctx, string icon, string tooltip) : base(ctx)
        {
            // Configurar el icono
            this.Directory = this.WebView.Browser.Directory;
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
                ThrowIfDisposed();
                return _ToolTip;
            } 
            set
            {
                ThrowIfDisposed();

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
                ThrowIfDisposed();
                return _Icon;
            }
            set
            {
                ThrowIfDisposed();

                IntPtr newHandle;
                string fullPath = value;
                
                if (!string.IsNullOrEmpty(value))
                {
                    if (!Path.IsPathFullyQualified(value))
                        fullPath = Path.Combine(Directory, value);

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
                ThrowIfDisposed();
                return _Visible;
            }
            set
            {
                ThrowIfDisposed();

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
            ThrowIfDisposed();
            this.Visible = true;
        }

        public void Hide()
        {
            ThrowIfDisposed();
            this.Visible = false;
        }

        #region ShowNotification

        public void ShowNotification(string title, string message, string iconSource) // Puede ser: "system", "warning", "error", o ruta de archivo
        {
            IntPtr hIcon = Helper.GetNotificationIcon(iconSource, Directory);

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

        #region Fire Events
        private void FireMouseDown(string button, POINT pos)
        {
            this.MouseDown?.Invoke(this.WebView, button, pos.X, pos.Y);
        }
       
        private void FireMouseUp(string button,POINT pos)
        {
            this.MouseUp?.Invoke(this.WebView, button, pos.X, pos.Y);
        }

        private void FireClick(string button, POINT pos)
        {
            if (!this.IsClick)
                return;

            this.Click?.Invoke(this.WebView, button, pos.X, pos.Y);
        }

        private void FireDoubleClick(string button, POINT pos)
        {
            this.DoubleClick?.Invoke(this.WebView, button, pos.X, pos.Y);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.WebView.Window.Raw -= Window_Raw;

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

    }
}