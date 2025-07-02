using NotifyIcon.Win32.Enums;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace NotifyIcon.Win32.Structs
{
    // LayoutKind.Sequential: Indica que los campos se ordenan secuencialmente en memoria, como en la estructura nativa.
    // CharSet = CharSet.Unicode: Especifica que las cadenas se manejan como Unicode(UTF-16).
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct NOTIFYICONDATA
    {
        /// <summary>
        /// Tamaño de la estructura en bytes.
        /// <para>
        /// Inicialización: Debe asignarse usando Marshal.SizeOf(typeof(NOTIFYICONDATA)) para garantizar compatibilidad con versiones de Windows.
        /// </para>
        /// </summary>
        public int cbSize;

        /// <summary>
        /// Handle de la ventana que recibe mensajes asociados al icono (ej: clics, movimientos del mouse).
        /// </summary>
        public IntPtr hWnd;

        /// <summary>
        /// Identificador único del icono. Útil si una aplicación maneja múltiples iconos.
        /// </summary>
        public int uID;

        /// <summary>
        /// Máscara de bits que indica qué campos son válidos
        /// </summary>
        public NotifyIconFlag uFlags;

        /// <summary>
        /// ID del mensaje personalizado enviado a hWnd cuando ocurre un evento (ej: clic en el icono).
        /// </summary>
        public int uCallbackMessage;

        /// <summary>
        /// Handle del icono mostrado en la bandeja. Usar Icon.Handle para íconos de recursos.
        /// </summary>
        public IntPtr hIcon;

        /// <summary>
        /// Tooltip/texto asociado al icono.
        /// <para>
        /// Tamaño máximo: 128 caracteres Unicode (incluyendo el nulo final).
        /// </para>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;

        /// <summary>
        /// Estado del icono.
        /// <para>
        /// NIS_HIDDEN (0x1): Oculta el icono.
        /// </para>
        /// <para>
        /// NIS_SHAREDICON (0x2): Icono compartido (Windows 7+).
        /// </para>
        /// </summary>
        public int dwState;

        /// <summary>
        /// Máscara para dwState. Indica qué bits de dwState se deben aplicar.
        /// </summary>
        public int dwStateMask;

        /// <summary>
        /// Texto del globo de información (notificación emergente).
        /// <para>
        /// Tamaño máximo: 256 caracteres Unicode.
        /// </para>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;

        /// <summary>
        /// Tiempo de visualización del globo (en milisegundos) o versión de la estructura.
        /// <para>
        /// Para usar versión 4 (Windows Vista+), asignar NOTIFYICON_VERSION_4 = 4.
        /// </para>
        /// </summary>
        public int uTimeoutOrVersion;

        /// <summary>
        /// Título del globo de información.
        /// <para>
        /// Tamaño máximo: 64 caracteres Unicode.
        /// </para>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;

        /// <summary>
        /// Icono y comportamiento del globo
        /// </summary>
        public int dwInfoFlags;

        /// <summary>
        /// GUID único para identificar el icono. Permite persistencia incluso si hWnd cambia (Windows Vista+).
        /// </summary>
        public Guid guidItem;

        /// <summary>
        /// Handle de un icono personalizado para el globo de información (usado si dwInfoFlags = NIIF_USER).
        /// </summary>
        public IntPtr hBalloonIcon;
    }
}