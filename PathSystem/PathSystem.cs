using WV;
using WV.Attributes;
using WV.Interfaces;

namespace PathSystem
{
    [Singleton]
    public class PathSystem : Plugin
    {
        public PathSystem(IWebView webView) : base(webView)
        {
        }

        #region PROPS

        public string RandomFileName
        {
            get
            {
                Plugin.ThrowDispose(this);
                return Path.GetRandomFileName();
            }
        }

        #endregion

        #region METHODS

        public string? ChangeExtension(string? path, string? extension)
        {
            Plugin.ThrowDispose(this);
            return Path.ChangeExtension(path, extension);
        }

        public bool Exists(string? path)
        {
            Plugin.ThrowDispose(this);
            return Path.Exists(path);
        }

        public string? GetDirectoryName(string? path)
        {
            Plugin.ThrowDispose(this);
            return Path.GetDirectoryName(path);
        }

        public string? GetExtension(string? path)
        {
            Plugin.ThrowDispose(this);
            return Path.GetExtension(path);
        }

        public string? GetFileName(string? path)
        {
            Plugin.ThrowDispose(this);
            return Path.GetFileName(path);
        }

        public string? GetFileNameWithoutExtension(string? path)
        {
            Plugin.ThrowDispose(this);
            return Path.GetFileNameWithoutExtension(path);
        }

        public bool IsPathFullyQualified(string path)
        {
            Plugin.ThrowDispose(this);
            return Path.IsPathRooted(path);
        }

        public bool HasExtension(string path)
        {
            Plugin.ThrowDispose(this);
            return Path.HasExtension(path);
        }

        public string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public string Join(params string?[] paths)
        {
            Plugin.ThrowDispose(this);
            return Path.Join(paths);
        }

        public string GetRelativePath(string relativeTo, string path)
        {
            Plugin.ThrowDispose(this);
            return Path.GetRelativePath(relativeTo, path);
        }

        public string TrimEndingDirectorySeparator(string path)
        {
            Plugin.ThrowDispose(this);
            return Path.TrimEndingDirectorySeparator(path);
        }

        public bool EndsInDirectorySeparator(string path)
        {
            Plugin.ThrowDispose(this);
            return Path.EndsInDirectorySeparator(path);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            //throw new NotImplementedException();
        }
    }
}