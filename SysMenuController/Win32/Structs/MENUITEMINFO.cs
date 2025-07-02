using System.Runtime.InteropServices;

namespace SystemMenu.Win32.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MENUITEMINFO
    {
        /// <summary>
        /// Tamaño del struct en bytes. Debe inicializarse antes de usar.
        /// </summary>
        public int cbSize;

        /// <summary>
        /// Máscara de bits que indica qué campos son válidos. MenuItemInfoMask enums
        /// </summary>
        public int fMask;

        /// <summary>
        /// Tipo de ítem (texto, separador, etc.).
        /// </summary>
        public int fType;

        /// <summary>
        /// Estado del ítem (habilitado, deshabilitado, marcado).
        /// </summary>
        public int fState;

        /// <summary>
        /// Identificador único del ítem (usado en eventos). [uMsg]
        /// </summary>
        public int wID;

        /// <summary>
        /// Handle a un submenú asociado al ítem.
        /// </summary>
        public IntPtr hSubMenu;

        /// <summary>
        /// Handle a bitmap para estado "marcado".
        /// </summary>
        public IntPtr hbmpChecked;

        /// <summary>
        /// Handle a bitmap para estado "no marcado".
        /// </summary>
        public IntPtr hbmpUnchecked;

        /// <summary>
        /// Datos personalizados asociados al ítem.
        /// </summary>
        public IntPtr dwItemData;

        /// <summary>
        /// Texto del ítem o datos personalizados. [Solo lectura]
        /// </summary>
        public string dwTypeData;

        /// <summary>
        /// Longitud del texto (en caracteres) para dwTypeData.
        /// </summary>
        public int cch;

        /// <summary>
        /// Handle a un bitmap mostrado junto al ítem.
        /// </summary>
        public IntPtr hbmpItem;
    }
}