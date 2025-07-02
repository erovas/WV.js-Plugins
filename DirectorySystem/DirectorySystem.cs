using WV;
using WV.Attributes;
using WV.Interfaces;

namespace DirectorySystem
{
    [Singleton]
    public class DirectorySystem : Plugin
    {
        public DirectorySystem(IWebView webView) : base(webView)
        {
        }

        #region PROPS

        public string CurrentDirectory
        {
            get
            {
                Plugin.ThrowDispose(this);
                return Directory.GetCurrentDirectory();
            }
            set
            {
                Plugin.ThrowDispose(this);
                Directory.SetCurrentDirectory(value);
            }
        }

        public string[] LogicalDrives
        {
            get
            {
                Plugin.ThrowDispose(this);
                return Directory.GetLogicalDrives();
            }
        }

        #endregion

        #region METHODS

        public void Create(string path)
        {
            Plugin.ThrowDispose(this);
            Directory.CreateDirectory(path);
        }

        public bool Exists(string path)
        {
            Plugin.ThrowDispose(this);
            return Directory.Exists(path);
        }

        public void Delete(string path, bool recursive = false)
        {
            Plugin.ThrowDispose(this);
            Directory.Delete(path, recursive);
        }

        public void Move(string sourceDirName, string destDirName)
        {
            Plugin.ThrowDispose(this);
            Directory.Move(sourceDirName, destDirName);
        }

        public string GetDirectoryRoot(string path)
        {
            Plugin.ThrowDispose(this);
            return Directory.GetDirectoryRoot(path);
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            Plugin.ThrowDispose(this);
            Directory.SetCreationTime(path, creationTime);
        }

        public DateTime GetCreationTime(string path)
        {
            Plugin.ThrowDispose(this);
            return Directory.GetCreationTime(path);
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            Plugin.ThrowDispose(this);
            Directory.SetLastWriteTime(path, lastWriteTime);
        }

        public DateTime GetLastWriteTime(string path)
        {
            Plugin.ThrowDispose(this);
            return Directory.GetLastWriteTime(path);
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            Plugin.ThrowDispose(this);
            Directory.SetLastAccessTime(path, lastAccessTime);
        }

        public DateTime GetLastAccessTime(string path)
        {
            Plugin.ThrowDispose(this);
            return Directory.GetLastAccessTime(path);
        }

        public string[] GetFiles(string path, string searchPattern = "*", string searchOption = "TopDirectoryOnly")
        {
            Plugin.ThrowDispose(this);

            if (Enum.TryParse(searchOption, out SearchOption sOptions))
                return Directory.GetFiles(path, searchPattern, sOptions);

            throw new Exception("Incorrect SearchOption");
        }

        public string[] GetDirectories(string path, string searchPattern = "*", string searchOption = "TopDirectoryOnly")
        {
            Plugin.ThrowDispose(this);

            if (Enum.TryParse(searchOption, out SearchOption sOptions))
                return Directory.GetDirectories(path, searchPattern, sOptions);

            throw new Exception("Incorrect SearchOption");
        }

        public string[] GetFileSystemEntries(string path, string searchPattern = "*", string searchOption = "TopDirectoryOnly")
        {
            Plugin.ThrowDispose(this);

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