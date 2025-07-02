﻿namespace SystemMenu.Win32.Enums
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-appendmenua
    /// <summary>
    /// For AppendMenu
    /// </summary>
    internal enum AppendMenuFlag : int
    {
        /// <summary>
        /// Uses a bitmap as the menu item. The lpNewItem parameter contains a handle to the bitmap.
        /// </summary>
        MF_BITMAP = 0x00000004,

        /// <summary>
        /// Places a check mark next to the menu item. If the application provides check-mark bitmaps 
        /// this flag displays the check-mark bitmap next to the menu item.
        /// </summary>
        MF_CHECKED = 0x00000008,

        /// <summary>
        /// Disables the menu item so that it cannot be selected, but the flag does not gray it.
        /// </summary>
        MF_DISABLED = 0x00000002,

        /// <summary>
        /// Enables the menu item so that it can be selected, and restores it from its grayed state.
        /// </summary>
        MF_ENABLED = 0x00000000,

        /// <summary>
        /// Disables the menu item and grays it so that it cannot be selected.
        /// </summary>
        MF_GRAYED = 0x00000001,

        /// <summary>
        /// Functions the same as the MF_MENUBREAK flag for a menu bar. For a drop-down menu, submenu, 
        /// or shortcut menu, the new column is separated from the old column by a vertical line.
        /// </summary>
        MF_MENUBARBREAK = 0x00000020,

        /// <summary>
        /// Places the item on a new line (for a menu bar) or in a new column (for a drop-down menu, 
        /// submenu, or shortcut menu) without separating columns.
        /// </summary>
        MF_MENUBREAK = 0x00000040,

        /// <summary>
        /// Specifies that the item is an owner-drawn item. Before the menu is displayed for the first time, 
        /// the window that owns the menu receives a WM_MEASUREITEM message to retrieve the width and height 
        /// of the menu item. The WM_DRAWITEM message is then sent to the window procedure of the owner 
        /// window whenever the appearance of the menu item must be updated.
        /// </summary>
        MF_OWNERDRAW = 0x00000100,

        /// <summary>
        /// Specifies that the menu item opens a drop-down menu or submenu. The uIDNewItem parameter 
        /// specifies a handle to the drop-down menu or submenu. This flag is used to add a menu name 
        /// to a menu bar, or a menu item that opens a submenu to a drop-down menu, submenu, 
        /// or shortcut menu.
        /// </summary>
        MF_POPUP = 0x00000010,

        /// <summary>
        /// Draws a horizontal dividing line. This flag is used only in a drop-down menu, submenu, 
        /// or shortcut menu. The line cannot be grayed, disabled, or highlighted. 
        /// The lpNewItem and uIDNewItem parameters are ignored.
        /// </summary>
        MF_SEPARATOR = 0x00000800,

        /// <summary>
        /// Specifies that the menu item is a text string; the lpNewItem parameter is a pointer to the string.
        /// </summary>
        MF_STRING = 0x00000000,

        /// <summary>
        /// Does not place a check mark next to the item (default). If the application supplies check-mark bitmaps, 
        /// this flag displays the clear bitmap next to the menu item.
        /// </summary>
        MF_UNCHECKED = 0x00000000
    }
}