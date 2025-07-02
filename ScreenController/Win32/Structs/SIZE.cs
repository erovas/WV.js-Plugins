using System.Runtime.InteropServices;

namespace ScreenController.Win32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SIZE
    {
        public int cx;
        public int cy;
    }
}