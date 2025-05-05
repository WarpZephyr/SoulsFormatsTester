using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SoulsFormatsTester.Logging
{
    internal class AppLogger : IDisposable
    {
        private readonly Logger AppLog;
        private readonly StreamWriter? FileLog;
        private readonly object LogLock;
        private bool disposedValue;

        public AppLogger(Logger appLog, StreamWriter? fileLog)
        {
            LogLock = new object();
            AppLog = appLog;
            FileLog = fileLog;

            lock (LogLock)
            {
                FileLog?.WriteLine($"[File Log Started: {DateTime.Now:MM-dd-yyyy-hh:mm:ss}]");
            }
        }

        public AppLogger(Logger appLog) : this(appLog, null) { }

        public void Write(string value)
        {
            lock (LogLock)
            {
                AppLog.Write(value);
                FileLog?.Write(value);
            }
        }

        public void WriteLine(string value)
        {
            lock (LogLock)
            {
                AppLog.WriteLine(value);
                FileLog?.WriteLine(value);
            }
        }

        public void WriteLine()
        {
            lock (LogLock)
            {
                AppLog.WriteLine();
                FileLog?.WriteLine();
            }
        }

        public void DirectWrite(string value)
        {
            lock (LogLock)
            {
                AppLog.DirectWrite(value);
                FileLog?.Write(value);
            }
        }

        public void DirectWriteLine(string value)
        {
            lock (LogLock)
            {
                AppLog.DirectWriteLine(value);
                FileLog?.WriteLine(value);
            }
        }

        public void DirectWriteLine()
        {
            lock (LogLock)
            {
                AppLog.DirectWriteLine();
                FileLog?.WriteLine();
            }
        }

        public void Flush()
        {
            lock (LogLock)
            {
                AppLog.Flush();
                FileLog?.Flush();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pause()
        {
            lock (LogLock)
            {
                Console.ReadKey();
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    FileLog?.WriteLine($"[File Log Ended: {DateTime.Now:MM-dd-yyyy-hh:mm:ss}]");

                    Flush();
                    AppLog.Dispose();
                    FileLog?.Dispose();
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
