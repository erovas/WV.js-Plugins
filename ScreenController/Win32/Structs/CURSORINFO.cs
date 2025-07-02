using System.Runtime.InteropServices;

namespace ScreenController.Win32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }
}