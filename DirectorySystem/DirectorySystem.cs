using WV;
using WV.Interfaces;

namespace DirectorySystem
{
    public class DirectorySystem : Plugin
    {
        public DirectorySystem(IPluginContext ct) : base(ct)
        {
        }

        #region PROPS

        public string CurrentDirectory
        {
            get
            {
                ThrowIfDisposed();
                return Directory.GetCurrentDirectory();
            }
            set
            {
                ThrowIfDisposed();
                Directory.SetCurrentDirectory(value);
            }
        }

        public string[] LogicalDrives
        {
            get
            {
                ThrowIfDisposed();
                return Directory.GetLogicalDrives();
            }
        }

        #endregion

        #region METHODS

        public void Create(string path)
        {
            ThrowIfDisposed();
            Directory.CreateDirectory(path);
        }

        public bool Exists(string path)
        {
            ThrowIfDisposed();
            return Directory.Exists(path);
        }

        public void Delete(string path, bool recursive = false)
        {
            ThrowIfDisposed();
            Directory.Delete(path, recursive);
        }

        public void Move(string sourceDirName, string destDirName)
        {
            ThrowIfDisposed();
            Directory.Move(sourceDirName, destDirName);
        }

        public string GetDirectoryRoot(string path)
        {
            ThrowIfDisposed();
            return Directory.GetDirectoryRoot(path);
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            ThrowIfDisposed();
            Directory.SetCreationTime(path, creationTime);
        }

        public DateTime GetCreationTime(string path)
        {
            ThrowIfDisposed();
            return Directory.GetCreationTime(path);
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            ThrowIfDisposed();
            Directory.SetLastWriteTime(path, lastWriteTime);
        }

        public DateTime GetLastWriteTime(string path)
        {
            ThrowIfDisposed();
            return Directory.GetLastWriteTime(path);
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            ThrowIfDisposed();
            Directory.SetLastAccessTime(path, lastAccessTime);
        }

        public DateTime GetLastAccessTime(string path)
        {
            ThrowIfDisposed();
            return Directory.GetLastAccessTime(path);
        }

        public string[] GetFiles(string path, string searchPattern = "*", string searchOption = "TopDirectoryOnly")
        {
            ThrowIfDisposed();

            if (Enum.TryParse(searchOption, out SearchOption sOptions))
                return Directory.GetFiles(path, searchPattern, sOptions);

            throw new Exception("Incorrect SearchOption");
        }

        public string[] GetDirectories(string path, string searchPattern = "*", string searchOption = "TopDirectoryOnly")
        {
            ThrowIfDisposed();

            if (Enum.TryParse(searchOption, out SearchOption sOptions))
                return Directory.GetDirectories(path, searchPattern, sOptions);

            throw new Exception("Incorrect SearchOption");
        }

        public string[] GetFileSystemEntries(string path, string searchPattern = "*", string searchOption = "TopDirectoryOnly")
        {
            ThrowIfDisposed();

            if (Enum.TryParse(searchOption, out SearchOption sOptions))
                return Directory.GetFileSystemEntries(path, searchPattern, sOptions);

            throw new Exception("Incorrect SearchOption");
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            //throw new NotImplementedException();
        }
    }
}