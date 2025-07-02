using ScreenController.Win32.Enums;
using ScreenController.Win32.Structs;

namespace ScreenController.Helpers
{
    internal class HScreenInfo
    {
        #region Propiedades Básicas

        /// <summary>
        /// Nombre interno del dispositivo de visualización
        /// Ejemplo: \\.\DISPLAY1, \\.\DISPLAY2
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del hardware (ID del monitor)
        /// Ejemplo: MONITOR\DELA07A\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001
        /// </summary>
        public string DeviceID { get; set; } = string.Empty;

        /// <summary>
        /// Ruta de registro del dispositivo en Windows
        /// Ejemplo: \Registry\Machine\System\CurrentControlSet\Control\Video\{...}
        /// </summary>
        public string DeviceKey { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo del monitor
        /// Ejemplo: Dell U2414H, Samsung Odyssey G9
        /// </summary>
        public string DeviceString { get; set; } = string.Empty;

        #endregion

        //==============================//

        #region Propiedades Geométricas

        /// <summary>
        /// Área total del monitor en coordenadas virtuales
        ///  Left;   // Coordenada X izquierda
        ///  Top;    // Coordenada Y superior
        ///  Right;  // Coordenada X derecha
        ///  Bottom; // Coordenada Y inferior
        /// </summary>
        public RECT MonitorArea { get; set; }

        /// <summary>
        /// Área útil (excluye barras de tareas)
        /// Misma estructura que MonitorArea
        /// </summary>
        public RECT WorkArea { get; set; }

        /// <summary>
        /// Posición horizontal en el espacio virtual
        /// Valor relativo al punto (0,0) del escritorio
        /// </summary>
        public int PositionX { get; set; }

        /// <summary>
        /// Posición vertical en el espacio virtual
        /// Valor relativo al punto (0,0) del escritorio
        /// </summary>
        public int PositionY { get; set; }

        #endregion

        //==============================//

        #region Propiedades de Resolución

        /// <summary>
        /// Ancho de la pantalla en píxeles (resolución horizontal)
        /// </summary>
        public int PixelWidth { get; set; }

        /// <summary>
        /// Alto de la pantalla en píxeles (resolución vertical)
        /// </summary>
        public int PixelHeight { get; set; }

        /// <summary>
        /// Profundidad de color
        /// Valores típicos: 32 (TrueColor+Alpha), 24 (TrueColor), 16 (HighColor)
        /// </summary>
        public int BitsPerPixel { get; set; }

        #endregion

        //==============================//

        #region Propiedades de Rendimiento

        /// <summary>
        /// Tasa de refresco en Hertz (Hz)
        /// Ejemplos: 60, 75, 120, 144, 240
        /// </summary>
        public int RefreshRate { get; set; }

        /// <summary>
        /// Puntos por pulgada (DPI) en eje horizontal
        /// </summary>
        public uint DpiX { get; set; }

        /// <summary>
        /// Puntos por pulgada (DPI) en eje vertical
        /// </summary>
        public uint DpiY { get; set; }

        /// <summary>
        /// Factor de escalado calculado
        /// Ejemplo: 1.0 = 96 DPI (100%), 1.5 = 144 DPI (150%)
        /// </summary>
        public float ScalingFactor => DpiX / 96.0f;

        #endregion

        //==============================//

        /// <summary>
        /// Indica si es la pantalla principal
        /// Equivalente a: (StateFlags & 0x4) != 0
        /// </summary>
        public bool IsPrimary { get; set; }
        
        public StateFlags StateFlags { get; set; }
        
    }
}