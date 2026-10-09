using WV;
using WV.Interfaces;

namespace DataStorage
{
    public class DataStorage : Plugin
    {
        private Dictionary<string, object?> Data { get; set; }

        public DataStorage(IPluginContext ctx) : base(ctx)
        {
            this.Data = App.DataStorage;
        }

        #region PROPS

        public int Count {
            get
            {
                ThrowIfDisposed();
                return this.Data.Count;
            }
        }

        public string[] Keys
        {
            get
            {
                ThrowIfDisposed();
                return this.Data.Keys.ToArray();
            }
        }

        public object?[] Values
        {
            get { 
                ThrowIfDisposed();
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
                ThrowIfDisposed();
                return this.Data[key];
            }
            set
            {
                ThrowIfDisposed();
                this.Data[key] = value;
            }
        }

        #endregion

        #region METHODS

        public void Clear()
        {
            ThrowIfDisposed();
            this.Data.Clear();
        }

        public bool Exists(string name)
        {
            ThrowIfDisposed();
            return string.IsNullOrEmpty(name) ? false : this.Data.ContainsKey(name);
        }

        public object? Get(string name)
        {
            ThrowIfDisposed();

            if (Exists(name))
                return this.Data[name];

            return null;
        }

        public bool Set(string name, object? value)
        {
            ThrowIfDisposed();

            if (!Exists(name))
                return false;

            this.Data[name] = value;
            return true;
        }

        public bool Add(string name, object? value)
        {
            ThrowIfDisposed();

            if (Exists(name))
                return false;

            this.Data[name] = value;
            return true;
        }

        public bool Remove(string name)
        {
            ThrowIfDisposed();

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
