using System.Runtime.InteropServices;

namespace MouseController.Win32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTDEVICE 
    {
        public ushort usUsagePage;
        public ushort usUsage; 
        public uint dwFlags; 
        public IntPtr hwndTarget; 
    }
}