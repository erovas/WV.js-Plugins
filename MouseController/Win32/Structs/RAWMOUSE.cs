using System.Runtime.InteropServices;

namespace MouseController.Win32.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct RAWMOUSE 
    {
        [FieldOffset(0)]
        public ushort usFlags;

        // Union entre ulButtons y (usButtonFlags + usButtonData)
        [FieldOffset(4)]
        public uint ulButtons; 

        [FieldOffset(4)]
        public ushort usButtonFlags;

        [FieldOffset(6)]
        public ushort usButtonData;  // Delta del wheel aquí

        [FieldOffset(8)]
        public int lLastX;           // Movimiento relativo X

        [FieldOffset(12)]
        public int lLastY;           // Movimiento relativo Y

        [FieldOffset(16)]
        public uint ulRawButtons;
    }
}