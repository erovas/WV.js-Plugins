namespace TaskbarManager.Win32.Enums
{
    internal enum FLASHWFLAG : uint
    {
        /// <summary>
        /// Detener el parpadeo
        /// </summary>
        FLASHW_STOP = 0x00000000,

        /// <summary>
        /// Parpadear la barra de título
        /// </summary>
        FLASHW_CAPTION = 0x00000001,

        /// <summary>
        /// Parpadear el icono de la barra de tareas
        /// </summary>
        FLASHW_TRAY = 0x00000002,

        /// <summary>
        /// Parpadear ambos, icono y barra
        /// </summary>
        FLASHW_ALL = 0x00000003,

        /// <summary>
        /// Parpadea continuamente, hasta que FLASHW_STOP sea establecido
        /// </summary>
        FLASHW_TIMER = 0x00000004,

        /// <summary>
        /// Parpadear hasta que la ventana esté en primer plano
        /// </summary>
        FLASHW_TIMERNOFG = 0x0000000C
    }
}