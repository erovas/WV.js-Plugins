using System.Runtime.InteropServices;

namespace MouseController.Win32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}