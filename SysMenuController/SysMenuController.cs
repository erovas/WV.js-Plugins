using System.Runtime.InteropServices;
using SystemMenu.Win32.Structs;
using SystemMenu.Win32.Enums;
using System.ComponentModel;
using SystemMenu.Win32;
using WV.Interfaces;
using WV.Attributes;
using WV;

namespace SystemMenu
{
    [Singleton]
    public class SysMenuController : Plugin
    {
        #region PRIVATE PROPS

        private IntPtr _hCustomMenu { get; }
        private Dictionary<int, SysItem> _systemItems { get; } = new Dictionary<int, SysItem>();
        private Dictionary<int, SysItem> _customItems { get; } = new Dictionary<int, SysItem>();
        private Dictionary<int, string> _IdTitles { get; } = new Dictionary<int, string>();

        private IDGenerator IDGenerator { get; }

        private IntPtr Handle { get; }

        #endregion

        public SysMenuController(IWebView webView) : base(webView)
        {
            this.IDGenerator = new IDGenerator();
            this._hCustomMenu = User32.CreatePopupMenu();
            this.Handle = User32.FindWindow(this.WebView.UID, null);

            // Obtener los items del sistema
            var SysMenuHandle = User32.GetSystemMenu(this.Handle, false);

            for (int i = 0; i < User32.GetMenuItemCount(SysMenuHandle); i++)
            {
                int id = User32.GetMenuItemID(SysMenuHandle, i);
                string? title = null;
                MENUITEMINFO mii = new MENUITEMINFO();

                if (id != -1) // Si no es un separador, obtener datos
                {
                    mii.cbSize = Marshal.SizeOf(mii);
                    mii.fMask = (int)(MenuItemInfoMask.MIIM_STATE | MenuItemInfoMask.MIIM_ID | MenuItemInfoMask.MIIM_STRING | MenuItemInfoMask.MIIM_BITMAP);
                    mii.dwTypeData = new string(' ', 256);
                    mii.cch = 256;
                    User32.GetMenuItemInfo(SysMenuHandle, i, true, ref mii);
                    title = mii.dwTypeData;
                }

                this._systemItems[id] = new SysItem()
                {
                    Id = id,
                    Title = title,
                    Position = i,
                    Mii = mii,
                    Visible = true
                };
            }

            // Construir el Menu
            RebuildMenu();

            this.WebView.Window.Raw += Window_Raw;

            // Agregar controlador al evento State del WebView, para controlar activación de los SystemItems
            this.WebView.Window.StateChanged += Win_StateChangedEvent;

            // Lanzar evento para asegurar correcta "enabilidad" de los items "nativos"
            this.Win_StateChangedEvent(this.WebView, this.WebView.Window.State, "");
        }

        private void Window_Raw(IWebView sender, object[] args, ref bool handled)
        {
            if (this.Disposed)
                return;

            IntPtr hWnd = (IntPtr)args[0];
            uint uMsg = (uint)args[1];
            IntPtr wParam = (IntPtr)args[2];
            IntPtr lParam = (IntPtr)args[3];

            handled = true;

            switch (uMsg)
            {
                case WinMsg.WM_SYSMENU: //Boton izquierdo en icono barra de tareas

                    // No mostrar menu si la ventana esta Hide()
                    if (!this.Visible)
                        break;

                    // Evitar que se muestre el menú nativo
                    POINT pt;
                    User32.GetCursorPos(out pt);
                    this.ShowMenu(pt.X, pt.Y, false);
                    break; 

                case WinMsg.WM_NCRBUTTONDOWN: // Click derecho en área no cliente

                    // No mostrar menu si la ventana esta Hide()
                    // Por si simulan el click por codigo
                    if (!this.Visible)
                        break;

                    int hitTest = wParam.ToInt32();
                    if (hitTest == HitTest.HTCAPTION || hitTest == HitTest.HTSYSMENU)
                    {
                        // Bloquear el menú original
                        User32.GetCursorPos(out pt);
                        this.ShowMenu(pt.X, pt.Y, false);
                        break;
                    }

                    // Quizas fue click en otra area en donde NO se despliega el SysMenu
                    handled = false;
                    break;

                case WinMsg.WM_SYSCOMMAND: // Interceptar Alt+Space

                    if ((wParam.ToInt32() & 0xFFF0) != (int)SysCommand.SC_KEYMENU)
                    {
                        handled = false; 
                        break;
                    }

                    if (!this.Visible)
                        break;

                    // Bloquear el menú original
                    this.ShowMenu(4, 29);
                    break; 


                case WinMsg.WM_COMMAND: // Procesar ítems del menú

                    // Quizas haya otros menus custom en algun otro plugin
                    // evitar "caparlos"
                    handled = false;  

                    int menuId = wParam.ToInt32();
                    switch (menuId)
                    {

                        case (int)SysCommand.SC_CLOSE:
                            this.WebView.Window.Close();
                            break;

                        case (int)SysCommand.SC_MINIMIZE:
                            this.WebView.Window.Minimize();
                            break;

                        case (int)SysCommand.SC_MAXIMIZE:
                            this.WebView.Window.Maximize();
                            break;

                        case (int)SysCommand.SC_RESTORE:
                            this.WebView.Window.Restore();
                            break;

                        case (int)SysCommand.SC_MOVE:
                            User32.SendMessage(hWnd, WinMsg.WM_SYSCOMMAND, (IntPtr)SysCommand.SC_MOVE, IntPtr.Zero); // WM_SYSCOMMAND with SC_MOVE
                            break;

                        case (int)SysCommand.SC_SIZE:
                            User32.SendMessage(hWnd, WinMsg.WM_SYSCOMMAND, (IntPtr)SysCommand.SC_SIZE, IntPtr.Zero); // WM_SYSCOMMAND with SC_MOVE
                            break;

                        default:
                            if (this._customItems.TryGetValue(menuId, out var item))
                                item.Callback?.Execute(this._IdTitles[menuId]);

                            break;
                    }
                    break;

                default:
                    handled = false;
                    break;
            }
        }

        private void Win_StateChangedEvent(IWebView sender, WV.Enums.WindowState state, string stateText)
        {
            switch (state)
            {
                case WV.Enums.WindowState.Minimized:
                    this.SysEnableItem(SysCommand.SC_RESTORE, true);
                    this.SysEnableItem(SysCommand.SC_MOVE, false);
                    this.SysEnableItem(SysCommand.SC_SIZE, false);
                    this.SysEnableItem(SysCommand.SC_MINIMIZE, false);
                    this.SysEnableItem(SysCommand.SC_MAXIMIZE, true);
                    break;

                case WV.Enums.WindowState.Normalized:
                    this.SysEnableItem(SysCommand.SC_RESTORE, false);
                    this.SysEnableItem(SysCommand.SC_MOVE, true);
                    this.SysEnableItem(SysCommand.SC_SIZE, true);
                    this.SysEnableItem(SysCommand.SC_MINIMIZE, true);
                    this.SysEnableItem(SysCommand.SC_MAXIMIZE, true);
                    break;

                case WV.Enums.WindowState.Maximized:
                    this.SysEnableItem(SysCommand.SC_RESTORE, true);
                    this.SysEnableItem(SysCommand.SC_MOVE, true);
                    this.SysEnableItem(SysCommand.SC_SIZE, false);
                    this.SysEnableItem(SysCommand.SC_MINIMIZE, true);
                    this.SysEnableItem(SysCommand.SC_MAXIMIZE, false);
                    break;
            }
        }

        #region PROPS

        public bool Visible { get; set; } = true;

        public bool CloseItem
        {
            get => this.GetSystemItemState(SysCommand.SC_CLOSE);
            set => this.UpdateSystemItem(SysCommand.SC_CLOSE, value);
        }

        public bool MaximizeItem
        {
            get => this.GetSystemItemState(SysCommand.SC_MAXIMIZE);
            set => this.UpdateSystemItem(SysCommand.SC_MAXIMIZE, value);
        }

        public bool MinimizeItem
        {
            get => this.GetSystemItemState(SysCommand.SC_MINIMIZE);
            set => this.UpdateSystemItem(SysCommand.SC_MINIMIZE, value);
        }

        public bool MoveItem
        {
            get => this.GetSystemItemState(SysCommand.SC_MOVE);
            set => this.UpdateSystemItem(SysCommand.SC_MOVE, value);
        }

        public bool SizeItem
        {
            get => this.GetSystemItemState(SysCommand.SC_SIZE);
            set => this.UpdateSystemItem(SysCommand.SC_SIZE, value);
        }

        public bool RestoreItem
        {
            get => this.GetSystemItemState(SysCommand.SC_RESTORE);
            set => this.UpdateSystemItem(SysCommand.SC_RESTORE, value);
        }


        #endregion

        #region METHODS

        public void AddItem(string title, object callback, bool enable = true)
        {
            Plugin.ThrowDispose(this);

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException("'" + nameof(title) + "' cannot be null or empty string");

            if (this._IdTitles.ContainsValue(title))
                throw new Exception("The item '" + title + "' already exists");

            // Agregar separador si es el primer ítem
            if (this._IdTitles.Count == 0)
            {
                User32.AppendMenu(this._hCustomMenu, AppendMenuFlag.MF_SEPARATOR, 0, null);
                this.IDGenerator.separatorPosition = User32.GetMenuItemCount(this._hCustomMenu) - 1;
            }

            // Generar nuevo ID y agregar ítem
            int newId = this.IDGenerator.GetNewIdItem();
            AppendMenuFlag flag = AppendMenuFlag.MF_STRING;

            if (!enable)
                flag |= AppendMenuFlag.MF_GRAYED;

            User32.AppendMenu(this._hCustomMenu, flag, newId, title);

            this._IdTitles[newId] = title;
            this._customItems[newId] = new SysItem
            {
                Id = newId,
                Title = title,
                //Visible = true,
                //Position = 0,
                Callback = IJSFunction.Create(callback),
                Enable = enable
            };
        }

        public void RemoveItem(string title)
        {
            Plugin.ThrowDispose(this);

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException("'" + nameof(title) + "' cannot be null or empty string");

            if (!this._IdTitles.ContainsValue(title))
                return;

            int id = this._IdTitles.FirstOrDefault(x => object.Equals(x.Value, title)).Key;
            IJSFunction? callback = this._customItems[id].Callback;

            User32.RemoveMenu(this._hCustomMenu, id, RemoveMenuFlag.MF_BYCOMMAND);
            this.IDGenerator.ReleaseIdItem(id);
            callback?.Dispose();

            this._IdTitles.Remove(id);
            this._customItems.Remove(id);

            // Eliminar separador si no hay más ítems
            if (this._IdTitles.Count == 0 && this.IDGenerator.separatorPosition != -1)
            {
                User32.RemoveMenu(this._hCustomMenu, this.IDGenerator.separatorPosition, RemoveMenuFlag.MF_BYPOSITION);
                this.IDGenerator.separatorPosition = -1;
            }
        }

        public void EnableItem(string title, bool enable)
        {
            Plugin.ThrowDispose(this);

            if (!this._IdTitles.ContainsValue(title))
                throw new Exception($"'{title}' no exists");

            int id = this._IdTitles.FirstOrDefault(x => x.Value.Equals(title)).Key;
            SysItem item = this._customItems[id];
            item.Enable = enable;
            User32.EnableMenuItem(this._hCustomMenu, id, EnableMenuItemFlag.MF_BYCOMMAND | (enable ? EnableMenuItemFlag.MF_ENABLED : EnableMenuItemFlag.MF_GRAYED));
        }

        public void ShowMenu(int x = 0, int y = 0, bool relative = true)
        {
            Plugin.ThrowDispose(this);

            if (relative && User32.GetWindowRect(this.Handle, out RECT rect))
            {
                x += rect.X;
                y += rect.Y;
            }

            User32.TrackPopupMenuEx(this._hCustomMenu, TrackPopupMenuFlag.TPM_LEFTALIGN, x, y, this.Handle, IntPtr.Zero);
        }

        #endregion

        private void UpdateSystemItem(SysCommand sc, bool value)
        {
            Plugin.ThrowDispose(this);
            int v = (int)sc;

            if (this._systemItems[v].Visible == value)
                return;

            this._systemItems[v].Visible = value;

            this.RebuildMenu();
        }

        private bool GetSystemItemState(SysCommand sc)
        {
            Plugin.ThrowDispose(this);
            return this._systemItems[(int)sc].Visible;
        }

        private void RebuildMenu()
        {
            // Vaciar el Menu completamente
            for (int i = User32.GetMenuItemCount(this._hCustomMenu) - 1; i >= 0; i--)
                User32.RemoveMenu(this._hCustomMenu, i, RemoveMenuFlag.MF_BYPOSITION);

            // Reconstruir items del sistema
            foreach (var item in this._systemItems.Values)
                if (item.Visible)
                    if (item.Id != 0)
                        User32.InsertMenuItem(this._hCustomMenu, item.Position, true, ref item.Mii);
                    else
                        User32.AppendMenu(this._hCustomMenu, AppendMenuFlag.MF_SEPARATOR, item.Id, item.Title);

            // Reconstruir custom items
            if (this._IdTitles.Count == 0)
                return;

            // Agregar separador para el primer custom item
            User32.AppendMenu(this._hCustomMenu, AppendMenuFlag.MF_SEPARATOR, 0, null);
            this.IDGenerator.separatorPosition = User32.GetMenuItemCount(this._hCustomMenu) - 1;

            foreach (var item in this._customItems.Values)
                User32.AppendMenu(this._hCustomMenu, item.Enable ? AppendMenuFlag.MF_STRING : AppendMenuFlag.MF_STRING | AppendMenuFlag.MF_GRAYED, item.Id, item.Title);
        }

        private void SysEnableItem(SysCommand sc, bool enable)
        {
            SysItem item = this._systemItems[(int)sc];
            item.Enable = enable;
            item.Mii.fState = (int)(enable ? MenuItemState.MFS_ENABLED : MenuItemState.MFS_DISABLED);

            // Si esta visible, activarlo/desactivarlo visualmente
            if (this.GetSystemItemState(sc))
                User32.EnableMenuItem(this._hCustomMenu, (int)sc, EnableMenuItemFlag.MF_BYCOMMAND | (enable ? EnableMenuItemFlag.MF_ENABLED : EnableMenuItemFlag.MF_GRAYED));
        }

        protected override void Dispose(bool disposing)
        {
            if(this.Disposed) 
                return;

            this.WebView.Window.Raw -= Window_Raw;
            this.WebView.Window.StateChanged -= Win_StateChangedEvent;

            this._customItems.Clear();
            this._systemItems.Clear();
            this._IdTitles.Clear();

            // Destruir el menu
            if (!User32.DestroyMenu(this._hCustomMenu))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}