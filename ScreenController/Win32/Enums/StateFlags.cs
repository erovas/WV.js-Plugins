using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenController.Win32.Enums
{
    internal enum StateFlags : uint
    {
        /// <summary>
        /// Dispositivo activo y en uso
        /// </summary>
        DISPLAY_DEVICE_ACTIVE = 1,

        /// <summary>
        /// Parte de un grupo multi-adapter
        /// </summary>
        DISPLAY_DEVICE_MULTI_DRIVER = 2,

        /// <summary>
        /// Dispositivo primario (monitor principal)
        /// </summary>
        DISPLAY_DEVICE_PRIMARY_DEVICE = 4,

        /// <summary>
        /// Está reflejando otro dispositivo
        /// </summary>
        DISPLAY_DEVICE_MIRRORING_DRIVER = 8,

        /// <summary>
        /// Compatible con modo VGA
        /// </summary>
        DISPLAY_DEVICE_VGA_COMPATIBLE = 16,

        /// <summary>
        /// Dispositivo removible (ej: USB)
        /// </summary>
        DISPLAY_DEVICE_REMOVABLE = 32,

        /// <summary>
        /// Controlador acelerado
        /// </summary>
        DISPLAY_DEVICE_ACC_DRIVER = 64,

        /// <summary>
        /// Tiene modos de resolución limitados
        /// </summary>
        DISPLAY_DEVICE_MODESPRUNED = 128,

        /// <summary>
        /// Controlador de dispositivo de pantalla remota
        /// </summary>
        DISPLAY_DEVICE_RDPUDD = 256,

        /// <summary>
        /// Dispositivo remoto (ej: RDP)
        /// </summary>
        DISPLAY_DEVICE_REMOTE = 512,

        /// <summary>
        /// Actualmente desconectado
        /// </summary>
        DISPLAY_DEVICE_DISCONNECT = 1024,

        /// <summary>
        /// Compatible con Terminal Services
        /// </summary>
        DISPLAY_DEVICE_TS_COMPATIBLE = 2048

    }
}
