namespace SystemMenu.Win32.Enums
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    internal enum MenuItemInfoMask : uint
    {
        // -----------------------------------------------
        // Valores estándar (comunes)
        // -----------------------------------------------

        /// <summary>
        /// Habilita el campo <b>fState</b> (estado del ítem: habilitado, marcado, etc.).
        /// </summary>
        MIIM_STATE = 0x00000001,

        /// <summary>
        /// Habilita el campo <b>wID</b> (identificador único del ítem).
        /// </summary>
        MIIM_ID = 0x00000002,

        /// <summary>
        /// Habilita el campo <b>hSubMenu</b> (handle a un submenú asociado al ítem).
        /// </summary>
        MIIM_SUBMENU = 0x00000004,

        /// <summary>
        /// Habilita los campos <b>hbmpChecked</b> y <b>hbmpUnchecked</b> (bitmaps para estados de check).
        /// </summary>
        MIIM_CHECKMARKS = 0x00000008,

        /// <summary>
        /// Habilita el campo <b>dwTypeData</b> (texto del ítem) y <b>cch</b> (longitud del texto).
        /// </summary>
        MIIM_STRING = 0x00000040,

        /// <summary>
        /// Habilita el campo <b>fType</b> (tipo de ítem: texto, separador, etc.).
        /// </summary>
        MIIM_FTYPE = 0x00000100,

        // -----------------------------------------------
        // Valores avanzados o menos usados
        // -----------------------------------------------

        /// <summary>
        /// Habilita el campo <b>dwItemData</b> (datos personalizados asociados al ítem).
        /// </summary>
        MIIM_DATA = 0x00000020,

        /// <summary>
        /// Habilita el campo <b>hbmpItem</b> (bitmap mostrado junto al ítem).
        /// </summary>
        MIIM_BITMAP = 0x00000080,

        /// <summary>
        /// (OBSOLETO) Usar <b>MIIM_FTYPE</b> y <b>MIIM_STRING</b> en su lugar.
        /// </summary>
        [Obsolete("Usar MIIM_FTYPE y MIIM_STRING")]
        MIIM_TYPE = 0x00000010,

        /// <summary>
        /// Aplica cambios al ítem y a todos sus submenús (usado en operaciones como SetMenuItemInfo).
        /// </summary>
        MIIM_APPLYTOSUBMENUS = 0x80000000
    }
}
