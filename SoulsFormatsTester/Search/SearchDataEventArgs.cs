using System;
using System.IO;

namespace SoulsFormatsTester.Search
{
    internal class SearchDataEventArgs : EventArgs, IDisposable
    {
        private bool disposedValue;

        public SearchData Data { get; init; }
        public string Name
            => Data.Name;
        public string Container
            => Data.Container;
        public string Format
            => Data.Format;
        public byte[]? Bytes
            => Data.Bytes;
        public Stream? Stream
            => Data.Stream;

        public SearchDataEventArgs(SearchData data)
        {
            Data = data;
        }

        public SearchDataEventArgs(string name, string container, string format, byte[] bytes)
        {
            Data = new SearchData(name, container, format, bytes);
        }

        public SearchDataEventArgs(string name, string container, string format, Stream stream, bool leaveStreamOpen)
        {
            Data = new SearchData(name, container, format, stream, leaveStreamOpen);
        }

        public SearchDataEventArgs(string name, string container, string format, Stream stream) : this(name, container, format, stream, false) { }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Data.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
