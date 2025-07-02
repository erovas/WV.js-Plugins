using NotifyIcon.Win32;
using NotifyIcon.Win32.Structs;
using WV;

namespace NotifyIcon
{
    internal static class Helper
    {
        private const int IDI_APPLICATION = 32512;
        private const int IDI_WARNING = 32515;
        private const int IDI_ERROR = 32513;
        private const int IDI_INFO = 32516;

        private const int NIIF_USER = 0x00000004;
        private const int NIIF_INFO = 0x00000001;
        private const int NIIF_WARNING = 0x00000002;
        private const int NIIF_ERROR = 0x00000003;

        public static IntPtr GetDefaultIconHandle()
        {
            // Intentar obtener el icono de aplicación por defecto
            IntPtr newHandle = User32.LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION);

            // Fallback a icono de shell32.dll si falla
            if (newHandle == IntPtr.Zero)
                newHandle = Shell32.ExtractIcon(IntPtr.Zero, "shell32.dll", 2); // DEFAULT_ICON_INDEX = 2, Índice del icono de aplicación genérico

            return newHandle;
        }

        public static IntPtr GetNotificationIcon(string iconSource)
        {
            if (string.IsNullOrEmpty(iconSource))
                return IntPtr.Zero;

            switch (iconSource.ToLower())
            {
                case "system":
                    return User32.LoadIcon(IntPtr.Zero, (IntPtr)IDI_APPLICATION);

                case "warning":
                    return User32.LoadIcon(IntPtr.Zero, (IntPtr)IDI_WARNING);

                case "error":
                    return User32.LoadIcon(IntPtr.Zero, (IntPtr)IDI_ERROR);

                case "info":
                    return User32.LoadIcon(IntPtr.Zero, (IntPtr)IDI_INFO);

                default:
                    string fullPath = iconSource;

                    if (string.IsNullOrEmpty(iconSource))
                        return GetDefaultIconHandle();

                    if (!Path.IsPathFullyQualified(iconSource))
                        fullPath = AppManager.SrcPath + "/" + iconSource;

                    if (!File.Exists(fullPath))
                        throw new FileNotFoundException("File not found: '" + iconSource + "'");

                    // 1 == IMAGE_ICON; 0x00000010 == LR_LOADFROMFILE
                    return User32.LoadImage(IntPtr.Zero, fullPath, 1, 0, 0, 0x00000010);
            }
        }

        public static int GetIconFlags(string iconSource)
        {
            return iconSource.ToLower() switch
            {
                "warning" => NIIF_WARNING,
                "error" => NIIF_ERROR,
                "info" => NIIF_INFO,
                _ => NIIF_USER
            };
        }

        public static POINT GetCursorPosition()
        {
            if(User32.GetCursorPos(out POINT result))
                return result;

            return new POINT { X = 0, Y = 0 };
        }
    }
}
