using System;
using System.IO;

namespace SoulsFormatsTester.Search
{
    internal class SearchData : IDisposable
    {
        public string Name { get; init; }
        public string Container { get; init; }
        public string Format { get; init; }
        public byte[]? Bytes { get; init; }
        public Stream? Stream { get; init; }
        private readonly bool LeaveStreamOpen;
        private bool disposedValue;

        public SearchData(string name, string container, string format, byte[] bytes)
        {
            Name = name;
            Container = container;
            Format = format;
            Bytes = bytes;
            Stream = null;
        }

        public SearchData(string name, string container, string format, Stream stream, bool leaveStreamOpen)
        {
            Name = name;
            Container = container;
            Format = format;
            Stream = stream;
            LeaveStreamOpen = leaveStreamOpen;
        }

        public SearchData(string name, string container, string format, Stream stream) : this(name, container, format, stream, false) { }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!LeaveStreamOpen)
                    {
                        Stream?.Dispose();
                    }
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
