﻿namespace TaskbarManager.Win32.Enums
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles
    /// </summary>
    internal enum WinStyles : long
    {
        /// <summary>
        /// The window has a thin-line border
        /// </summary>
        WS_BORDER = 0x00800000L,

        /// <summary>
        /// The window has a title bar (includes the WS_BORDER style).
        /// </summary>
        WS_CAPTION = 0x00C00000L,

        /// <summary>
        /// The window is a child window. A window with this style cannot have a menu bar. 
        /// This style cannot be used with the WS_POPUP style.
        /// </summary>
        WS_CHILD = 0x40000000L,

        /// <summary>
        /// Same as the WS_CHILD style. 
        /// </summary>
        WS_CHILDWINDOW = 0x40000000L,

        /// <summary>
        /// Excludes the area occupied by child windows when drawing occurs within the parent window. 
        /// This style is used when creating the parent window.
        /// </summary>
        WS_CLIPCHILDREN = 0x02000000L,

        /// <summary>
        /// Clips child windows relative to each other; that is, when a particular child window 
        /// receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping 
        /// child windows out of the region of the child window to be updated. 
        /// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible,
        /// when drawing within the client area of a child window, to draw within the client area
        /// of a neighboring child window.receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping
        /// child windows out of the region of the child window to be updated. 
        /// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible,
        /// when drawing within the client area of a child window, to draw within the client area
        /// of a neighboring child window.
        /// </summary>
        WS_CLIPSIBLINGS = 0x04000000L,

        /// <summary>
        /// The window is initially disabled. 
        /// A disabled window cannot receive input from the user. 
        /// To change this after a window has been created, use the EnableWindow function.
        /// </summary>
        WS_DISABLED = 0x08000000L,

        /// <summary>
        /// The window has a border of a style typically used with dialog boxes. 
        /// A window with this style cannot have a title bar.
        /// </summary>
        WS_DLGFRAME = 0x00400000L,

        /// <summary>
        /// The window is the first control of a group of controls. 
        /// The group consists of this first control and all controls defined after it, up to
        /// the next control with the WS_GROUP style. The first control in each group usually
        /// has the WS_TABSTOP style so that the user can move from group to group. 
        /// The user can subsequently change the keyboard focus from one control in the group
        /// to the next control in the group by using the direction keys.
        /// You can turn this style on and off to change dialog box navigation.
        /// To change this style after a window has been created, use the SetWindowLong function.
        /// </summary>
        WS_GROUP = 0x00020000L,

        /// <summary>
        /// The window has a horizontal scroll bar.
        /// </summary>
        WS_HSCROLL = 0x00100000L,

        /// <summary>
        /// The window is initially minimized. Same as the WS_MINIMIZE style.
        /// </summary>
        WS_ICONIC = 0x20000000L,

        /// <summary>
        /// The window is initially maximized.
        /// </summary>
        WS_MAXIMIZE = 0x01000000L,

        /// <summary>
        /// The window has a maximize button. 
        /// Cannot be combined with the WS_EX_CONTEXTHELP style. 
        /// The WS_SYSMENU style must also be specified.
        /// </summary>
        WS_MAXIMIZEBOX = 0x00010000L,

        /// <summary>
        /// The window is initially minimized. Same as the WS_ICONIC style.
        /// </summary>
        WS_MINIMIZE = 0x20000000L,

        /// <summary>
        /// The window has a minimize button. 
        /// Cannot be combined with the WS_EX_CONTEXTHELP style. 
        /// The WS_SYSMENU style must also be specified.
        /// </summary>
        WS_MINIMIZEBOX = 0x00020000L,

        /// <summary>
        /// The window is an overlapped window. 
        /// An overlapped window has a title bar and a border. 
        /// Same as the WS_TILED style.
        /// </summary>
        WS_OVERLAPPED = 0x00000000L,


        // 0x00CF0000
        /// <summary>
        /// The window is an overlapped window. Same as the WS_TILEDWINDOW style.
        /// </summary>
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

        /// <summary>
        /// The window is a pop-up window. This style cannot be used with the WS_CHILD style.
        /// </summary>
        WS_POPUP = 0x80000000L,

        /// <summary>
        /// The window is a pop-up window. 
        /// The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.
        /// </summary>
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

        /// <summary>
        /// The window has a sizing border. Same as the WS_THICKFRAME style.
        /// </summary>
        WS_SIZEBOX = 0x00040000L,

        /// <summary>
        /// The window has a window menu on its title bar. The WS_CAPTION style must also be specified.
        /// </summary>
        WS_SYSMENU = 0x00080000L,

        /// <summary>
        /// The window is a control that can receive the keyboard focus when the user presses the TAB key. 
        /// Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.
        /// You can turn this style on and off to change dialog box navigation. To change this style
        /// after a window has been created, use the SetWindowLong function. For user-created windows and
        /// modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage
        /// function.
        /// </summary>
        WS_TABSTOP = 0x00010000L,

        /// <summary>
        /// The window has a sizing border. Same as the WS_SIZEBOX style.
        /// </summary>
        WS_THICKFRAME = 0x00040000L,

        /// <summary>
        /// The window is an overlapped window. 
        /// An overlapped window has a title bar and a border. 
        /// Same as the WS_OVERLAPPED style.
        /// </summary>
        WS_TILED = 0x00000000L,

        /// <summary>
        /// The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.
        /// </summary>
        WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

        /// <summary>
        /// The window is initially visible.
        /// This style can be turned on and off by using the ShowWindow or SetWindowPos function.
        /// </summary>
        WS_VISIBLE = 0x10000000L,

        /// <summary>
        /// The window has a vertical scroll bar.
        /// </summary>
        WS_VSCROLL = 0x00200000L
    }
}