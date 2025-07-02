namespace MouseController.Win32.Enums
{
    [Flags]
    internal enum MouseEventFlags
    {
        LEFT_DOWN = 0x02,
        LEFT_UP = 0x04,
        RIGHT_DOWN = 0x08,
        RIGHT_UP = 0x10,
        MIDDLE_DOWN = 0x20,
        MIDDLE_UP = 0x40,
        X_DOWN = 0x0080,
        X_UP = 0x0100,
        WHEEL = 0x0800,
        HWHEEL = 0x1000,
        ABSOLUTE = 0x8000
    }
}