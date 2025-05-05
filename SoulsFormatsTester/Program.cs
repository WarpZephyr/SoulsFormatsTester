#if DEBUG
#define CRASH_ON_EXCEPTION
#endif

using SoulsFormatsTester.Logging;
using SoulsFormatsTester.Search;
using SoulsFormatsTester.Test;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SoulsFormatsTester
{
    internal class Program
    {
        static readonly string AppFolder = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string FileLogPath = Path.Combine(AppFolder, "test.log");

        static void Main(string[] args)
        {
            var logger = new AppLogger(ConsoleLog.LoggerInstance, GetFileLog());
            if (args.Length < 1)
            {
                logger.DirectWriteLine("Please drag and drop folders and/or files onto the exe to use this tool.");
                logger.Dispose();
                Pause();
                return;
            }

            logger.DirectWriteLine("Starting search...");
            var searchEngine = new SearchEngine();
            var testEngine = new TestEngine(searchEngine, logger);
            foreach (string arg in args)
            {
#if !CRASH_ON_EXCEPTION
                try
                {
                    Process(testEngine, arg);
                }
                catch (Exception ex)
                {
                    ConsoleLog.WriteLine($"An exception error occurred on: {arg}\nException: {ex}");
                }
#else
                Process(testEngine, logger, arg);
#endif
            }

            testEngine.Wait();
            logger.DirectWriteLine("Finished searching.");
            logger.Dispose();
            testEngine.Dispose();
            Pause();
        }

        static void Process(TestEngine testEngine, AppLogger logger, string path)
        {
            if (Directory.Exists(path))
            {
                testEngine.TestFolder(path);
            }
            else if (File.Exists(path))
            {
                testEngine.TestFile(path);
            }
            else
            {
                logger.WriteLine($"Error: Not a file or folder: {path}");
            }
        }

        static StreamWriter? GetFileLog()
        {
            if (Directory.Exists(AppFolder))
            {
                return new StreamWriter(FileLogPath, false);
            }
            else
            {
                Console.WriteLine("Warning: Cannot find program folder, file log will be unavailable.");
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Pause()
            => Console.ReadKey(true);
    }
}
