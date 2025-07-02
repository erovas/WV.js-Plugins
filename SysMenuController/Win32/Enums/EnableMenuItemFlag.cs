namespace SystemMenu.Win32.Enums
{
    //https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enablemenuitem

    /// <summary>
    /// Flags for EnableMenuItem
    /// </summary>
    internal enum EnableMenuItemFlag : int   // Deberia ser uint
    {
        /// <summary>
        /// Indicates that uIDEnableItem gives the identifier of the menu item. 
        /// If neither the MF_BYCOMMAND nor MF_BYPOSITION flag is specified, 
        /// the MF_BYCOMMAND flag is the default flag.
        /// </summary>
        MF_BYCOMMAND = 0x00000000,

        /// <summary>
        /// Indicates that uIDEnableItem gives the zero-based relative position of the menu item.
        /// </summary>
        MF_BYPOSITION = 0x00000400,

        /// <summary>
        /// Indicates that the menu item is disabled, but not grayed, so it cannot be selected.
        /// </summary>
        MF_DISABLED = 0x00000002,

        /// <summary>
        /// Indicates that the menu item is enabled and restored from a grayed state so that it can be selected.
        /// </summary>
        MF_ENABLED = 0x00000000,

        /// <summary>
        /// Indicates that the menu item is disabled and grayed so that it cannot be selected.
        /// </summary>
        MF_GRAYED = 0x00000001
    }
}