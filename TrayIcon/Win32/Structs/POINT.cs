using System.Runtime.InteropServices;

namespace NotifyIcon.Win32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int X;
        public int Y;
    }
}