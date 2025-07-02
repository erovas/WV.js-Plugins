namespace SystemMenu.Win32.Enums
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-removemenu
    /// </summary>
    internal enum RemoveMenuFlag : int
    {
        /// <summary>
        /// Indicates that uPosition gives the identifier of the menu item. If neither the MF_BYCOMMAND 
        /// nor MF_BYPOSITION flag is specified, the MF_BYCOMMAND flag is the default flag.
        /// </summary>
        MF_BYCOMMAND = 0x00000000,


        /// <summary>
        /// Indicates that uPosition gives the zero-based relative position of the menu item.
        /// </summary>
        MF_BYPOSITION = 0x00000400

    }
}