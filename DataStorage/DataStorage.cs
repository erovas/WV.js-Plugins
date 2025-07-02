using WV;
using WV.Attributes;
using WV.Interfaces;

namespace DataStorage
{
    [Singleton]
    public class DataStorage : Plugin
    {
        private Dictionary<string, object?> Data { get; set; }

        public DataStorage(IWebView webView) : base(webView)
        {
            this.Data = AppManager.DataStorage;
        }

        #region PROPS

        public int Count {
            get
            {
                Plugin.ThrowDispose(this);
                return this.Data.Count;
            }
        }

        public string[] Keys
        {
            get
            {
                Plugin.ThrowDispose(this);
                return this.Data.Keys.ToArray();
            }
        }

        public object?[] Values
        {
            get { 
                Plugin.ThrowDispose(this);
                return this.Data.Values.ToArray();
            }
        }

        #endregion

        #region INDEXER

        [System.Runtime.CompilerServices.IndexerName("Items")]
        public object? this[string key]
        {
            get
            {
                Plugin.ThrowDispose(this);
                return this.Data[key];
            }
            set
            {
                Plugin.ThrowDispose(this);
                this.Data[key] = value;
            }
        }

        #endregion

        #region METHODS

        public void Clear()
        {
            Plugin.ThrowDispose(this);
            this.Data.Clear();
        }

        public bool Exists(string name)
        {
            Plugin.ThrowDispose(this);
            return string.IsNullOrEmpty(name) ? false : this.Data.ContainsKey(name);
        }

        public object? Get(string name)
        {
            Plugin.ThrowDispose(this);

            if (Exists(name))
                return this.Data[name];

            return null;
        }

        public bool Set(string name, object? value)
        {
            Plugin.ThrowDispose(this);

            if (!Exists(name))
                return false;

            this.Data[name] = value;
            return true;
        }

        public bool Add(string name, object? value)
        {
            Plugin.ThrowDispose(this);

            if (Exists(name))
                return false;

            this.Data[name] = value;
            return true;
        }

        public bool Remove(string name)
        {
            Plugin.ThrowDispose(this);

            if (Exists(name))
                return this.Data.Remove(name);

            return false;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            // Quitar referencia del dictionary original
            this.Data = new Dictionary<string, object?>();
        }
    }
}
