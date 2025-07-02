using ScreenController.Win32.Structs;

namespace ScreenController.Helpers
{
    internal class HCursorInfo
    {
        public POINT Position { get; set; }
        public SIZE Size { get; set; }
        public nint Handle { get; set; }
        public bool IsVisible { get; set; }
        public POINT HotSpot { get; set; }
        public byte[] CursorImage { get; set; } = new byte[0]; // Datos RGBA
    }
}