using System.Runtime.InteropServices;
using TaskbarManager.Win32.Structs;

namespace TaskbarManager.Win32
{
    internal static class User32
    {
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(nint hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetWindowLong(nint hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(nint hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern nint GetClassLongPtr(nint hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint LoadImage(nint hInstance, string lpFileName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32")]
        public static extern int FlashWindow(nint hwnd, bool bInvert);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);
    }
}