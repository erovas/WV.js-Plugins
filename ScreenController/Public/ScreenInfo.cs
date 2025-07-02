namespace ScreenController.Public
{
    public class ScreenInfo
    {

        #region Propiedades Básicas

        /// <summary>
        /// Nombre interno del dispositivo de visualización
        /// Ejemplo: \\.\DISPLAY1, \\.\DISPLAY2
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// Identificador único del hardware (ID del monitor)
        /// Ejemplo: MONITOR\DELA07A\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001
        /// </summary>
        public string DeviceID { get; }

        /// <summary>
        /// Ruta de registro del dispositivo en Windows
        /// Ejemplo: \Registry\Machine\System\CurrentControlSet\Control\Video\{...}
        /// </summary>
        public string DeviceKey { get; }

        /// <summary>
        /// Nombre descriptivo del monitor
        /// Ejemplo: Dell U2414H, Samsung Odyssey G9
        /// </summary>
        public string DeviceString { get; }

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
        public Rect MonitorArea { get; }

        /// <summary>
        /// Área útil (excluye barras de tareas)
        /// Misma estructura que MonitorArea
        /// </summary>
        public Rect WorkArea { get; }

        /// <summary>
        /// Posición horizontal en el espacio virtual
        /// Valor relativo al punto (0,0) del escritorio
        /// </summary>
        public int PositionX { get; }

        /// <summary>
        /// Posición vertical en el espacio virtual
        /// Valor relativo al punto (0,0) del escritorio
        /// </summary>
        public int PositionY { get; }

        #endregion

        //==============================//

        #region Propiedades de Resolución

        /// <summary>
        /// Ancho de la pantalla en píxeles (resolución horizontal)
        /// </summary>
        public int PixelWidth { get; }

        /// <summary>
        /// Alto de la pantalla en píxeles (resolución vertical)
        /// </summary>
        public int PixelHeight { get; }

        /// <summary>
        /// Profundidad de color
        /// Valores típicos: 32 (TrueColor+Alpha), 24 (TrueColor), 16 (HighColor)
        /// </summary>
        public int BitsPerPixel { get; }

        #endregion

        //==============================//

        #region Propiedades de Rendimiento

        /// <summary>
        /// Tasa de refresco en Hertz (Hz)
        /// Ejemplos: 60, 75, 120, 144, 240
        /// </summary>
        public int RefreshRate { get; }

        /// <summary>
        /// Puntos por pulgada (DPI) en eje horizontal
        /// </summary>
        public uint DpiX { get; }

        /// <summary>
        /// Puntos por pulgada (DPI) en eje vertical
        /// </summary>
        public uint DpiY { get; }

        /// <summary>
        /// Factor de escalado calculado
        /// Ejemplo: 1.0 = 96 DPI (100%), 1.5 = 144 DPI (150%)
        /// </summary>
        public float ScalingFactor { get; }

        #endregion

        //==============================//

        /// <summary>
        /// Indica si es la pantalla principal
        /// Equivalente a: (StateFlags & 0x4) != 0
        /// </summary>
        public bool IsPrimary { get; }

        public string[] StateFlags { get; }

        //==============================//

        internal ScreenInfo(string DeviceName, string DeviceID, string DeviceKey, string DeviceString, 
            Rect MonitorArea, Rect WorkArea, int PositionX, int PositionY, 
            int PixelWidth, int PixelHeight, int BitsPerPixel,
            int RefreshRate, uint DpiX, uint DpiY, float ScalingFactor,
            bool IsPrimary, string[] StateFlags)
        {
            this.DeviceName = DeviceName;
            this.DeviceID = DeviceID;
            this.DeviceKey = DeviceKey;
            this.DeviceString = DeviceString;
            this.MonitorArea = MonitorArea;
            this.WorkArea = WorkArea;
            this.PositionX = PositionX;
            this.PositionY = PositionY;
            this.PixelWidth = PixelWidth;
            this.PixelHeight = PixelHeight;
            this.BitsPerPixel = BitsPerPixel;
            this.RefreshRate = RefreshRate;
            this.DpiX = DpiX;
            this.DpiY = DpiY;
            this.ScalingFactor = ScalingFactor;
            this.IsPrimary = IsPrimary;
            this.StateFlags = StateFlags;
        }

    }
}
