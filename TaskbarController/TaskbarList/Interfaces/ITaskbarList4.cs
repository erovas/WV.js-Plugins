using System.Runtime.InteropServices;
using TaskbarManager.TaskbarList.Enums;
using TaskbarManager.TaskbarList.Structs;

namespace TaskbarManager.TaskbarList.Interfaces
{
    [ComImportAttribute()]
    [GuidAttribute("c43dc798-95d1-4bea-9030-bb99e2983a1a")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITaskbarList4
    {
        // ITaskbarList

        /// <summary>
        /// Inicializa el objeto de la barra de tareas. Debe llamarse antes de usar otros métodos. 
        /// Retorna un código de éxito/error (HRESULT), pero en .NET se marca con PreserveSig para 
        /// manejar el resultado manualmente.
        /// </summary>
        [PreserveSig]
        void HrInit();

        /// <summary>
        /// Agrega una ventana (identificada por su handle hwnd) a la barra de tareas como una pestaña nueva.
        /// </summary>
        /// <param name="hwnd"></param>
        [PreserveSig]
        void AddTab(IntPtr hwnd);

        /// <summary>
        /// Elimina la pestaña de una ventana de la barra de tareas (por ejemplo, cuando se cierra la ventana).
        /// </summary>
        /// <param name="hwnd"></param>
        [PreserveSig]
        void DeleteTab(IntPtr hwnd);

        /// <summary>
        /// Activa la pestaña de una ventana, llevándola al frente (como cuando el usuario hace clic en su ícono).
        /// </summary>
        /// <param name="hwnd"></param>
        [PreserveSig]
        void ActivateTab(IntPtr hwnd);

        /// <summary>
        /// Marca una ventana como "activa alternativamente", útil para situaciones como 
        /// previsualizaciones sin activar la ventana en primer plano.
        /// </summary>
        /// <param name="hwnd"></param>
        [PreserveSig]
        void SetActiveAlt(IntPtr hwnd);

        // ITaskbarList2

        /// <summary>
        /// Indica si una ventana está en modo pantalla completa. La barra de tareas puede ocultarse automáticamente.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="fFullscreen"></param>
        [PreserveSig]
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        // ITaskbarList3

        /// <summary>
        /// Muestra una barra de progreso en el ícono de la barra de tareas (ejemplo: descargas).
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="ullCompleted">Valor actual (ejemplo: 50).</param>
        /// <param name="ullTotal">Valor máximo (ejemplo: 100).</param>
        [PreserveSig]
        void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);

        /// <summary>
        /// Define el estado de la barra de progreso (normal, pausado, error, etc.) usando el enum eeTaskBarStatus.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="tbpFlags"></param>
        [PreserveSig]
        void SetProgressState(IntPtr hwnd, eeTaskBarStatus tbpFlags);

        /// <summary>
        /// Registra una pestaña secundaria (ejemplo: pestañas MDI) bajo una ventana principal para agruparlas en la barra de tareas.
        /// </summary>
        /// <param name="hwndTab"></param>
        /// <param name="hwndMDI"></param>
        [PreserveSig]
        void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);

        /// <summary>
        /// Elimina el registro de una pestaña previamente registrada.
        /// </summary>
        /// <param name="hwndTab"></param>
        [PreserveSig]
        void UnregisterTab(IntPtr hwndTab);

        /// <summary>
        /// Establece el orden de las pestañas en la barra de tareas. hwndTab se coloca antes de hwndInsertBefore.
        /// </summary>
        /// <param name="hwndTab"></param>
        /// <param name="hwndInsertBefore"></param>
        [PreserveSig]
        void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);

        /// <summary>
        /// Activa una pestaña y define su posición en el orden.
        /// </summary>
        /// <param name="hwndTab"></param>
        /// <param name="hwndInsertBefore"></param>
        /// <param name="dwReserved"></param>
        [PreserveSig]
        void SetTabActive(IntPtr hwndTab, IntPtr hwndInsertBefore, uint dwReserved);

        /// <summary>
        /// Añade botones a la miniatura de la barra de tareas (ejemplo: controles de reproducción de música).
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="cButtons">Número de botones.</param>
        /// <param name="pButtons">Array de botones (iconos y acciones).</param>
        /// <returns></returns>
        [PreserveSig]
        HResult ThumbBarAddButtons( IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] ThumbButton[] pButtons);

        /// <summary>
        /// Actualiza el estado o apariencia de los botones de la miniatura.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="cButtons"></param>
        /// <param name="pButtons"></param>
        /// <returns></returns>
        [PreserveSig]
        HResult ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] ThumbButton[] pButtons);

        /// <summary>
        /// Define una lista de imágenes (iconos) para los botones de la miniatura.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="himl"></param>
        [PreserveSig]
        void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);

        /// <summary>
        /// Muestra un icono superpuesto en el ícono de la barra de tareas (ejemplo: notificaciones).
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hIcon">Handle del icono.</param>
        /// <param name="pszDescription">Texto para accesibilidad.</param>
        [PreserveSig]
        void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

        /// <summary>
        /// Define un tooltip para la miniatura de la ventana al pasar el mouse.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="pszTip"></param>
        [PreserveSig]
        void SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

        /// <summary>
        /// Recorta una región de la ventana para mostrar en la miniatura.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="prcClip">Puntero a una estructura RECT que define el área.</param>
        [PreserveSig]
        void SetThumbnailClip(IntPtr hwnd, IntPtr prcClip);

        // ITaskbarList4
        /// <summary>
        /// Configura propiedades avanzadas de una pestaña, como marcar una pestaña como "privada" (ejemplo: navegación de incógnito).
        /// </summary>
        /// <param name="hwndTab"></param>
        /// <param name="stpFlags">Opciones del enum SetTabPropertiesOption.</param>
        void SetTabProperties(IntPtr hwndTab, SetTabPropertiesOption stpFlags);
    }

}