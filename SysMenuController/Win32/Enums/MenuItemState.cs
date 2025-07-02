namespace SystemMenu.Win32.Enums
{
    [Flags]
    internal enum MenuItemState : uint
    {
        /// <summary>
        /// Ítem habilitado (valor predeterminado). No se puede combinar con <b>MFS_DISABLED</b> o <b>MFS_GRAYED</b>.
        /// </summary>
        MFS_ENABLED = 0x00000000,

        /// <summary>
        /// Ítem deshabilitado (no interactuable).
        /// </summary>
        MFS_DISABLED = 0x00000003,

        /// <summary>
        /// Ítem marcado con una verificación (✔️). Usar con <b>MIIM_STATE</b> en <c>fMask</c>.
        /// </summary>
        MFS_CHECKED = 0x00000008,

        /// <summary>
        /// Ítem resaltado (como al pasar el mouse).
        /// </summary>
        MFS_HILITE = 0x00000080,

        /// <summary>
        /// Ítem deshabilitado y grisáceo (sinónimo de <b>MFS_DISABLED</b> en versiones modernas).
        /// </summary>
        MFS_GRAYED = 0x00000003, // Mismo valor que MFS_DISABLED

        /// <summary>
        /// Ítem predeterminado (se muestra en negrita, como "Abrir" en el menú contextual de un archivo).
        /// </summary>
        MFS_DEFAULT = 0x00001000,

        /// <summary>
        /// Obsoleto. Usar <b>MFS_CHECKED</b> en su lugar.
        /// </summary>
        [Obsolete("Usar MFS_CHECKED")]
        MFS_ACTIVE = 0x00000080, // Equivale a MFS_HILITE

        /// <summary>
        /// Obsoleto. Usar <b>MFS_HILITE</b>.
        /// </summary>
        [Obsolete("Usar MFS_HILITE")]
        MFS_SELECTED = 0x00000080,
    }
}