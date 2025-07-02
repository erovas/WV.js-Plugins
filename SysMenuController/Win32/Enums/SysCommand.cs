namespace SystemMenu.Win32.Enums
{
    /// <summary>
    /// System Command [SC]
    /// </summary>
    internal enum SysCommand
    {
        /// <summary>
        /// Restores the window to its normal position and size.
        /// </summary>
        SC_RESTORE = 0xF120,

        /// <summary>
        /// Moves the window.
        /// </summary>
        SC_MOVE = 0xF010,

        /// <summary>
        /// Sizes the window.
        /// </summary>
        SC_SIZE = 0xF000,

        /// <summary>
        /// Minimizes the window.
        /// </summary>
        SC_MINIMIZE = 0xF020,

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        SC_MAXIMIZE = 0xF030,

        /// <summary>
        /// Closes the window.
        /// </summary>
        SC_CLOSE = 0xF060,

        /// <summary>
        /// Retrieves the window menu as a result of a keystroke.
        /// </summary>
        SC_KEYMENU = 0xF100
    }
}