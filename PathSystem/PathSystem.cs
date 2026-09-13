using WV;
using WV.Attributes;
using WV.Interfaces;

namespace PathSystem
{
    [Singleton]
    public class PathSystem : Plugin
    {
        public PathSystem(IContext ctx) : base(ctx)
        {
        }

        #region PROPS

        public string RandomFileName
        {
            get
            {
                ThrowIfDisposed();
                return Path.GetRandomFileName();
            }
        }

        #endregion

        #region METHODS

        public string? ChangeExtension(string? path, string? extension)
        {
            ThrowIfDisposed();
            return Path.ChangeExtension(path, extension);
        }

        public bool Exists(string? path)
        {
            ThrowIfDisposed();
            return Path.Exists(path);
        }

        public string? GetDirectoryName(string? path)
        {
            ThrowIfDisposed();
            return Path.GetDirectoryName(path);
        }

        public string? GetExtension(string? path)
        {
            ThrowIfDisposed();
            return Path.GetExtension(path);
        }

        public string? GetFileName(string? path)
        {
            ThrowIfDisposed();
            return Path.GetFileName(path);
        }

        public string? GetFileNameWithoutExtension(string? path)
        {
            ThrowIfDisposed();
            return Path.GetFileNameWithoutExtension(path);
        }

        public bool IsPathFullyQualified(string path)
        {
            ThrowIfDisposed();
            return Path.IsPathRooted(path);
        }

        public bool HasExtension(string path)
        {
            ThrowIfDisposed();
            return Path.HasExtension(path);
        }

        public string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public string Join(params string?[] paths)
        {
            ThrowIfDisposed();
            return Path.Join(paths);
        }

        public string GetRelativePath(string relativeTo, string path)
        {
            ThrowIfDisposed();
            return Path.GetRelativePath(relativeTo, path);
        }

        public string TrimEndingDirectorySeparator(string path)
        {
            ThrowIfDisposed();
            return Path.TrimEndingDirectorySeparator(path);
        }

        public bool EndsInDirectorySeparator(string path)
        {
            ThrowIfDisposed();
            return Path.EndsInDirectorySeparator(path);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            //throw new NotImplementedException();
        }
    }
}