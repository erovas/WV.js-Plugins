namespace SystemMenu.Win32.Enums
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-trackpopupmenuex
    /// </summary>
    internal enum TrackPopupMenuFlag : uint
    {
        /// <summary>
        /// Positions the shortcut menu so that its left side is aligned with the coordinate specified by the x parameter.
        /// </summary>
        TPM_LEFTALIGN = 0x0000
    }
}