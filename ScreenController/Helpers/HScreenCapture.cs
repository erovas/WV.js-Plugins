using System.Runtime.InteropServices;

namespace ScreenController.Helpers
{
    internal class HScreenCapture
    {
        public nint Bits { get; }
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public int BitsPerPixel { get; }
        public byte[] RawData { get; }

        public HScreenCapture(nint bits, int width, int height, int stride, int bitsPerPixel)
        {
            Bits = bits;
            Width = width;
            Height = height;
            Stride = stride;
            BitsPerPixel = bitsPerPixel;

            // Copiar datos a un array manejado
            int bufferSize = Math.Abs(stride) * height;
            RawData = new byte[bufferSize];
            Marshal.Copy(bits, RawData, 0, bufferSize);
        }
    }
}