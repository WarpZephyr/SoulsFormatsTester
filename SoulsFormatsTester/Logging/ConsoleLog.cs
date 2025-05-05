using System;
using System.Runtime.CompilerServices;

namespace SoulsFormatsTester.Logging
{
    internal static class ConsoleLog
    {
        public static int WriteThreshold
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LoggerInstance.WriteThreshold;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => LoggerInstance.WriteThreshold = value;
        }

        public static int IntervalSeconds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LoggerInstance.IntervalSeconds;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => LoggerInstance.IntervalSeconds = value;
        }

        public static readonly Logger LoggerInstance;
        private static bool disposedValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(string value)
            => LoggerInstance.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(string value)
            => LoggerInstance.WriteLine(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine()
            => LoggerInstance.WriteLine();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DirectWrite(string value)
            => LoggerInstance.DirectWrite(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DirectWriteLine(string value)
            => LoggerInstance.DirectWriteLine(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DirectWriteLine()
            => LoggerInstance.DirectWriteLine();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Flush()
            => LoggerInstance.Flush();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pause()
            => Console.ReadKey(true);

        static ConsoleLog()
        {
            LoggerInstance = new Logger(50, 3, 3, true);
        }

        public static void DisposeLogger()
        {
            if (!disposedValue)
            {
                LoggerInstance.Dispose();
                disposedValue = true;
            }
        }
    }
}
