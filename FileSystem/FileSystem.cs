using WV;
using WV.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace FileSystem
{
    public class FileSystem : Plugin
    {
        public FileSystem(IPluginContext ctx) : base(ctx)
        {
        }

        #region METHODS

        public void Create(string path)
        {
            ThrowIfDisposed();
            var data = File.Create(path);
            data.Close();
        }

        public void Copy(string source, string destination, bool overwrite = false)
        {
            ThrowIfDisposed();
            File.Copy(source, destination, overwrite);
        }

        public bool Exists(string path)
        {
            ThrowIfDisposed();
            return File.Exists(path);
        }

        public void Delete(string path)
        {
            ThrowIfDisposed();
            File.Delete(path);
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            ThrowIfDisposed();
            File.SetCreationTime(path, creationTime);
        }

        public DateTime GetCreationTime(string path)
        {
            ThrowIfDisposed();
            return File.GetCreationTime(path);
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            ThrowIfDisposed();
            File.SetLastAccessTime(path, lastAccessTime);
        }

        public DateTime GetLastAccessTime(string path)
        {
            ThrowIfDisposed();
            return File.GetLastAccessTime(path);
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            ThrowIfDisposed();
            File.SetLastAccessTime(path, lastWriteTime);
        }

        public DateTime GetLastWriteTime(SafeFileHandle fileHandle)
        {
            ThrowIfDisposed();
            return File.GetLastWriteTime(fileHandle);
        }

        public void SetAttributes(string path, string[] fileAttributes)
        {
            ThrowIfDisposed();
            FileAttributes attributes = ArrayToAttributes(fileAttributes);
            File.SetAttributes(path, attributes);
        }

        public string[] GetAttributes(string path)
        {
            ThrowIfDisposed();
            return AttributesToArray(File.GetAttributes(path));
        }

        public void Move(string source, string destination, bool overwrite = false)
        {
            ThrowIfDisposed();
            File.Move(source, destination, overwrite);
        }

        public string ReadAllText(string path)
        {
            ThrowIfDisposed();
            return File.ReadAllText(path);
        }

        public string[] ReadAllLines(string path)
        {
            ThrowIfDisposed();
            return File.ReadAllLines(path);
        }

        public void WriteAllText(string path, string text)
        {
            ThrowIfDisposed();
            File.WriteAllText(path, text);
        }

        public void AppendAllText(string path, string text)
        {
            ThrowIfDisposed();
            File.AppendAllText(path, text);
        }

        public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName)
        {
            ThrowIfDisposed();
            File.Replace(sourceFileName, destinationFileName, destinationBackupFileName);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            //throw new NotImplementedException();
        }

        private static string[] AttributesToArray(FileAttributes value)
        {
            List<string> listaAtributos = new List<string>();

            // Recorrer todos los valores posibles de FileAttributes
            foreach (FileAttributes valor in Enum.GetValues(typeof(FileAttributes)))
                // Verificar si el atributo está presente usando operaciones bitwise
                if ((value & valor) == valor && valor != 0)
                    listaAtributos.Add(valor.ToString());

            return listaAtributos.ToArray();
        }

        private static FileAttributes ArrayToAttributes(string[] attributes)
        {
            FileAttributes resultado = 0;
            List<string> atributosInvalidos = new List<string>();

            foreach (string attr in attributes)
            {
                if (Enum.TryParse(attr, true, out FileAttributes atributoParseado))
                    // Verificar si es un único atributo válido (potencia de 2)
                    if (IsValidAttribute(atributoParseado))
                        resultado |= atributoParseado;
                    else
                        atributosInvalidos.Add(attr);
                else
                    atributosInvalidos.Add(attr);
            }

            if (atributosInvalidos.Count > 0)
                throw new Exception("Invalid attributes " + atributosInvalidos.ToString());

            return resultado;
        }

        private static bool IsValidAttribute(FileAttributes atributo)
        {
            uint valor = (uint)atributo;
            // Debe ser potencia de 2 y no ser 0
            return valor != 0 && (valor & (valor - 1)) == 0;
        }

    }
}
