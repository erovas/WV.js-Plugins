using WV;
using WV.Attributes;
using WV.Interfaces;
using TaskbarManager.Enums;
using TaskbarManager.Win32;
using TaskbarManager.Win32.Enums;
using TaskbarManager.TaskbarList;
using TaskbarManager.Win32.Structs;
using System.Runtime.InteropServices;
using TaskbarManager.TaskbarList.Enums;
using TaskbarManager.TaskbarList.Interfaces;

namespace TaskbarManager
{
    [Singleton]
    public sealed class TaskbarController : Plugin
    {
        private IntPtr Handle { get; }
        private ITaskbarList4 TaskbarList { get; }

        public TaskbarController(IWebView webView) : base(webView)
        {
            if(!(Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.CompareTo(new Version(6, 1)) >= 0))
                throw new Exception("Platform not supported");

            this.Handle = User32.FindWindow(this.WebView.UID, null);
            this.TaskbarList = (ITaskbarList4)new CTaskbarList();
            TaskbarList.HrInit();

            this.WebView.Window.Visible += Window_VisibleEvent;
        }

        private void Window_VisibleEvent(IWebView sender, bool isVisible)
        {
            // Cuando pasa de Invisible a Visible, hay que setear de nuevo el Tooltip
            if (isVisible && this.Visible)
                this.TaskbarList.SetThumbnailTooltip(this.Handle, this.Tooltip);
        }

        #region Status

        private TaskbarStatus _Status = TaskbarStatus.None;

        public TaskbarStatus Status
        {
            get 
            {
                Plugin.ThrowDispose(this);
                return _Status;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (this.Status == TaskbarStatus.Indeterminate && value == TaskbarStatus.Normal)
                    this.TaskbarList.SetProgressState(this.Handle, eeTaskBarStatus.None);
                
                _Status = value;

                this.TaskbarList.SetProgressState(this.Handle, (eeTaskBarStatus)Enum.Parse(typeof(eeTaskBarStatus), this.Status.ToString(), true));
                this.Progress = this.Progress;
            }
        }

        public string StatusText
        {
            get 
            {
                Plugin.ThrowDispose(this);
                return this.Status.ToString();
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (Enum.TryParse(value, out TaskbarStatus myStatus))
                    this.Status = myStatus;
            }
        }

        #endregion

        #region Progress

        private const int MaxValue = 100;
        private const int MinValue = 0;
        private int _Progress;

        public int Progress
        {
            get 
            { 
                Plugin.ThrowDispose(this); 
                return _Progress; 
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (value < MinValue)
                    value = MinValue;

                else if (value > MaxValue)
                    value = MaxValue;

                _Progress = value;

                if (this.Status == TaskbarStatus.Indeterminate || this.Status == TaskbarStatus.None)
                    return;

                this.TaskbarList.SetProgressValue(this.Handle, Convert.ToUInt64(value), Convert.ToUInt64(MaxValue));
            }
        }


        #endregion

        #region Visible

        private bool _Visible = true;
        public bool Visible { 
            get
            {
                Plugin.ThrowDispose(this);
                return this._Visible;
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (this.Visible == value)
                    return;

                if (value)
                    TaskbarList.AddTab(this.Handle);
                else
                    TaskbarList.DeleteTab(this.Handle);

                this._Visible = value;

                // Cuando pasa de Invisible a Visible, hay que setear de nuevo el Tooltip
                if(this.WebView.Window.IsVisible && value)
                    this.TaskbarList.SetThumbnailTooltip(this.Handle, this.Tooltip);
            }
        }

        #endregion

        #region Icon, IconSmall

        private const uint ICON_SMALL = 0;
        private const uint ICON_BIG = 1;
        private const uint IMAGE_ICON = 1;
        private const int GCLP_HICON = -14;
        private const int GCLP_HICONSM = -34;
        private const uint LR_LOADFROMFILE = 0x0010;
        private const uint LR_DEFAULTSIZE = 0x0040;
        private const uint WM_SETICON = 0x0080;

        private IntPtr _hIcon = IntPtr.Zero;
        private IntPtr _hIconSm = IntPtr.Zero;
        private string _Icon = string.Empty;
        private string _IconSmall = string.Empty;

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

                if (_Icon == value)
                    return;

                //IntPtr iconSet = IntPtr.Zero;

                if (string.IsNullOrEmpty(value))
                    _hIcon = DestroyIcon(_hIcon, GCLP_HICON);
                else
                    _hIcon = SetIcon(_hIcon, value, false);

                // Establecer icono
                User32.SendMessage(this.Handle, WM_SETICON, (IntPtr)ICON_BIG, _hIcon);
                _Icon = value;
            }
        }

        public string IconSmall
        {
            get 
            { 
                Plugin.ThrowDispose(this); 
                return _IconSmall; 
            }
            set
            {
                Plugin.ThrowDispose(this);

                if (_IconSmall == value)
                    return;

                //IntPtr iconSet = IntPtr.Zero;

                if (string.IsNullOrEmpty(value))
                    _hIconSm = DestroyIcon(_hIconSm, GCLP_HICONSM);
                else
                    _hIconSm = SetIcon(_hIconSm, value, true);

                // Establecer icono
                User32.SendMessage(this.Handle, WM_SETICON, (IntPtr)ICON_SMALL, _hIconSm);
                _IconSmall = value;
            }
        }


        private IntPtr DestroyIcon(IntPtr hIcon, int nIndex)
        {
            if (hIcon != IntPtr.Zero)
                User32.DestroyIcon(hIcon);

            // Quitar referencia
            hIcon = IntPtr.Zero;

            // Obtener icono por defecto
            IntPtr classIcon = User32.GetClassLongPtr(this.Handle, nIndex);

            return classIcon;
        }

        private static IntPtr SetIcon(IntPtr hIcon, string path, bool smallIcon)
        {
            string fullPath = path;

            if (!Path.IsPathFullyQualified(path))
                fullPath = AppManager.SrcPath + "/" + path;

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found: '" + path + "'");

            int cx = 256;
            int cy = 256;
            uint flags = LR_LOADFROMFILE;

            if (smallIcon)
            {
                cx = 16;
                cy = 16;
                flags |= LR_DEFAULTSIZE;
            }

            var newIcon = User32.LoadImage(
                IntPtr.Zero,
                fullPath,
                IMAGE_ICON,
                cx,
                cy,
                flags
            );

            if (newIcon == IntPtr.Zero)
                throw new Exception("Error setting icon");

            if (hIcon != IntPtr.Zero)
                User32.DestroyIcon(hIcon);

            //hIcon = newIcon;
            return newIcon;
        }

        #endregion

        #region Tooltip

        private string _Tooltip = string.Empty;

        public string Tooltip
        {
            get => _Tooltip; 
            set
            {
                if (this.Tooltip == value)
                    return;

                if(string.IsNullOrEmpty(value))
                    value = string.Empty;

                this.TaskbarList.SetThumbnailTooltip(this.Handle, value);
                _Tooltip = value;
            }
        }

        #endregion

        #region Blinking

        private bool _Blinking;
        public bool Blinking
        {
            get => _Blinking;
            set
            {
                Plugin.ThrowDispose(this);

                if (this.Blinking == value) 
                    return;

                _Blinking = value;

                FLASHWINFO fInfo = new FLASHWINFO();
                fInfo.cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO));
                fInfo.hwnd = this.Handle;
                fInfo.dwTimeout = 0; // Usar intervalo por defecto
                fInfo.uCount = (value ? uint.MaxValue : 0);
                fInfo.dwFlags = (value ? (uint)FLASHWFLAG.FLASHW_TRAY : (uint)FLASHWFLAG.FLASHW_STOP);
                User32.FlashWindowEx(ref fInfo);
            }
        }

        #endregion

        #region METHODS

        public void Hide()
        {
            this.Visible = false;
        }

        public void Show()
        {
            this.Visible = true;
        }

        public void StartBlinking()
        {
            this.Blinking = true;
        }

        public void StopBlinking()
        {
            this.Blinking = false;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if(this.Disposed)
                return;

            this.Status = TaskbarStatus.None;
            this.Progress = 0;
            this.Visible = true;

            this.Icon = string.Empty;
            this.IconSmall = string.Empty;

            this.Tooltip = string.Empty;
            this.Blinking = false;

            this.WebView.Window.Visible -= Window_VisibleEvent;

#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
            // Liberar objeto COM ITaskbarList4
            Marshal.FinalReleaseComObject(this.TaskbarList);
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
        }

    }
}