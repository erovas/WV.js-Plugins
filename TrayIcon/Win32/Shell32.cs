using NotifyIcon.Win32.Enums;
using NotifyIcon.Win32.Structs;
using System.Runtime.InteropServices;

namespace NotifyIcon.Win32
{
    internal static class Shell32
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool Shell_NotifyIcon(NotifyIconMessage dwMessage, ref NOTIFYICONDATA lpData);

        [DllImport("shell32.dll", EntryPoint = "ExtractIcon")]
        public static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);
    }
}