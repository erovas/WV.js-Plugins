using System.Runtime.InteropServices;

namespace SystemMenu.Win32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public readonly int X => this.Left;
        public readonly int Y => this.Top;
    }
}