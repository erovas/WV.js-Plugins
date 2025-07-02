using System.Runtime.InteropServices;

namespace ScreenController.Win32
{
    internal static class Shcore
    {
        [DllImport("shcore.dll")]
        public static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    }
}