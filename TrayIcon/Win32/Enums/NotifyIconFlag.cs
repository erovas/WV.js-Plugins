namespace NotifyIcon.Win32.Enums
{
    /// <summary>
    /// NIF- Notify Icon Flag
    /// <para>
    /// https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataa
    /// </para>
    /// </summary>
    internal enum NotifyIconFlag : int
    {
        /// <summary>
        /// The uCallbackMessage member is valid.
        /// </summary>
        NIF_MESSAGE = 0x00000001,

        /// <summary>
        /// The hIcon member is valid.
        /// </summary>
        NIF_ICON = 0x00000002,

        /// <summary>
        /// The szTip member is valid.
        /// </summary>
        NIF_TIP = 0x00000004,

        /// <summary>
        /// The dwState and dwStateMask members are valid.
        /// </summary>
        NIF_STATE = 0x00000008,

        /// <summary>
        /// Display a balloon notification. The szInfo, szInfoTitle, dwInfoFlags, and 
        /// uTimeout members are valid. Note that uTimeout is valid only in Windows 2000 
        /// and Windows XP.
        /// <para>
        /// To display the balloon notification, specify NIF_INFO and provide text in szInfo.
        /// </para>
        /// <para>
        /// To remove a balloon notification, specify NIF_INFO and provide an empty string through szInfo.
        /// </para>
        /// <para>
        /// To add a notification area icon without displaying a notification, do not set the NIF_INFO flag.
        /// </para>
        /// </summary>
        NIF_INFO = 0x00000010,

        /// <summary>
        /// <para>
        /// Windows 7 and later: The guidItem is valid.
        /// </para>
        /// <para>
        /// Windows Vista and earlier: Reserved.
        /// </para>
        /// </summary>
        NIF_GUID = 0x00000020,

        /// <summary>
        /// Windows Vista and later. If the balloon notification cannot be displayed immediately, discard it. 
        /// Use this flag for notifications that represent real-time information which would be meaningless 
        /// or misleading if displayed at a later time. For example, a message that states "Your telephone is ringing." 
        /// NIF_REALTIME is meaningful only when combined with the NIF_INFO flag.
        /// </summary>
        NIF_REALTIME = 0x00000040,

        /// <summary>
        /// Windows Vista and later. Use the standard tooltip. Normally, when uVersion is set to NOTIFYICON_VERSION_4, 
        /// the standard tooltip is suppressed and can be replaced by the application-drawn, pop-up UI. 
        /// If the application wants to show the standard tooltip with NOTIFYICON_VERSION_4, 
        /// it can specify NIF_SHOWTIP to indicate the standard tooltip should still be shown.
        /// </summary>
        NIF_SHOWTIP = 0x00000080
    }
}
