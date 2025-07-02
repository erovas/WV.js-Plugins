using SystemMenu.Win32.Structs;
using WV.Interfaces;

namespace SystemMenu
{
    internal class SysItem
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public bool Visible { get; set; } = true;

        public int Position { get; set; }

        public bool Enable { get; set; } = true;

        public IJSFunction? Callback { get; set; }

        public MENUITEMINFO Mii;
    }
}