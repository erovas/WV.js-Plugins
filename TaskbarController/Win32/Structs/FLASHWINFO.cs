using System.Runtime.InteropServices;

namespace TaskbarManager.Win32.Structs
{
    // Estructura para FlashWindowEx
    [StructLayout(LayoutKind.Sequential)]
    internal struct FLASHWINFO
    {
        public uint cbSize;    // Tamaño de la estructura
        public IntPtr hwnd;   // Handle de la ventana
        public uint dwFlags;  // Modo de parpadeo
        public uint uCount;   // Número de veces que parpadea (0 = infinito)
        public uint dwTimeout; // Intervalo entre parpadeos (ms)
    }
}