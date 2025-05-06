using SoulsFormats;
using SoulsFormats.Other;
using SoulsFormatsTester.Logging;
using SoulsFormatsTester.Search;
using System;
using System.Linq;
using System.Threading;

namespace SoulsFormatsTester.Test
{
    internal class TestEngine : IDisposable
    {
        private readonly object WorkCountLock;
        private int WorkCount;

        private readonly SearchEngine SearchEngine;
        private readonly AppLogger Log;
        private bool disposedValue;

        public TestEngine(SearchEngine searchEngine, AppLogger log)
        {
            Log = log;
            SearchEngine = searchEngine;
            SearchEngine.OnSearch += OnSearch;

            WorkCountLock = new object();
        }

        public void TestFolder(string folder)
        {
            Working();
            ThreadPool.QueueUserWorkItem((state) =>
            {
                SearchEngine.SearchFolder(folder);
                FinishedWork();
            });
        }

        public void TestFile(string file)
        {
            Working();
            ThreadPool.QueueUserWorkItem((state) =>
            {
                SearchEngine.SearchFile(file);
                FinishedWork();
            });
        }

        private void OnSearch(object? sender, SearchDataEventArgs e)
        {
            Working();
            ThreadPool.QueueUserWorkItem((state) =>
            {
                Test(e.Data);
                FinishedWork();
            });
        }

        private void Test(SearchData data)
        {
            if (data.Format == string.Empty)
            {
                Log.WriteLine($"Unknown: {data.Name}");
            }
            else
            {
                string result;
                switch (data.Format)
                {
                    case "DCX":
                        byte[] dcxRead;
                        DCX.Type dcxType;
                        if (data.Bytes != null)
                            dcxRead = DCX.Decompress(data.Bytes, out dcxType);
                        else if (data.Stream != null)
                            dcxRead = DCX.Decompress(data.Stream, out dcxType);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        DCX.Compress(dcxRead, dcxType);
                        result = "Success";
                        break;
                    case "BND2":
                        BND2 bnd2;
                        if (data.Bytes != null)
                            bnd2 = BND2.Read(data.Bytes);
                        else if (data.Stream != null)
                            bnd2 = BND2.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        BND2.Read(bnd2.Write());
                        result = "Success";
                        break;
                    case "BND3":
                        BND3 bnd3;
                        if (data.Bytes != null)
                            bnd3 = BND3.Read(data.Bytes);
                        else if (data.Stream != null)
                            bnd3 = BND3.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        BND3.Read(bnd3.Write());
                        result = "Success";
                        break;
                    case "BND4":
                        BND4 bnd4;
                        if (data.Bytes != null)
                            bnd4 = BND4.Read(data.Bytes);
                        else if (data.Stream != null)
                            bnd4 = BND4.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        BND4.Read(bnd4.Write());
                        result = "Success";
                        break;
                    case "FLVER0":
                        try
                        {
                            FLVER0 flver0;
                            if (data.Bytes != null)
                                flver0 = FLVER0.Read(data.Bytes);
                            else if (data.Stream != null)
                                flver0 = FLVER0.Read(data.Stream);
                            else
                                throw new Exception($"{nameof(SearchData)} returned no source data.");

                            FLVER0.Read(flver0.Write());
                            result = "Success";
                        }
                        catch
                        {
                            result = "Failure";
                        }
                        break;
                    case "FLVER2":
                        FLVER2 flver2;
                        if (data.Bytes != null)
                            flver2 = FLVER2.Read(data.Bytes);
                        else if (data.Stream != null)
                            flver2 = FLVER2.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        if (!flver2.Meshes.Any(m => m.VertexBuffers.Any(v => v.EdgeCompressed == true)))
                        {
                            FLVER2.Read(flver2.Write());
                        }

                        result = "Success";
                        break;
                    case "MDL4":
                        MDL4 mdl4;
                        if (data.Bytes != null)
                            mdl4 = MDL4.Read(data.Bytes);
                        else if (data.Stream != null)
                            mdl4 = MDL4.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        MDL4.Read(mdl4.Write());
                        result = "Success";
                        break;
                    case "SMD4":
                        SMD4 smd4;
                        if (data.Bytes != null)
                            smd4 = SMD4.Read(data.Bytes);
                        else if (data.Stream != null)
                            smd4 = SMD4.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        SMD4.Read(smd4.Write());
                        result = "Success";
                        break;
                    case "FMG":
                        FMG fmg;
                        if (data.Bytes != null)
                            fmg = FMG.Read(data.Bytes);
                        else if (data.Stream != null)
                            fmg = FMG.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        FMG.Read(fmg.Write());
                        result = "Success";
                        break;
                    case "TPF":
                        TPF tpf;
                        if (data.Bytes != null)
                            tpf = TPF.Read(data.Bytes);
                        else if (data.Stream != null)
                            tpf = TPF.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        TPF.Read(tpf.Write());
                        result = "Success";
                        break;
                    case "PARAM":
                        PARAM param;
                        if (data.Bytes != null)
                            param = PARAM.Read(data.Bytes);
                        else if (data.Stream != null)
                            param = PARAM.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        result = "Success";
                        break;
                    case "PARAMDEF":
                        PARAMDEF paramdef;
                        if (data.Bytes != null)
                            paramdef = PARAMDEF.Read(data.Bytes);
                        else if (data.Stream != null)
                            paramdef = PARAMDEF.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        PARAMDEF.Read(paramdef.Write());
                        result = "Success";
                        break;
                    case "DBP":
                        PARAMDBP dbp;
                        if (data.Bytes != null)
                            dbp = PARAMDBP.Read(data.Bytes);
                        else if (data.Stream != null)
                            dbp = PARAMDBP.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        PARAMDBP.Read(dbp.Write());
                        result = "Success";
                        break;
                    case "MTD":
                        MTD mtd;
                        if (data.Bytes != null)
                            mtd = MTD.Read(data.Bytes);
                        else if (data.Stream != null)
                            mtd = MTD.Read(data.Stream);
                        else
                            throw new Exception($"{nameof(SearchData)} returned no source data.");

                        MTD.Read(mtd.Write());
                        result = "Success";
                        break;
                    default:
                        result = "NoTest";
                        break;
                }

                Log.WriteLine($"[{data.Format}|{result}]: {data.Name}");
            } 

            data.Dispose();
        }

        private void Working()
        {
            lock (WorkCountLock)
            {
                WorkCount++;
            }
        }

        private void FinishedWork()
        {
            lock (WorkCountLock)
            {
                WorkCount--;
            }
        }

        public void Wait()
        {
            while (true)
            {
                lock (WorkCountLock)
                {
                    if (WorkCount < 1)
                    {
                        return;
                    }
                }

                Thread.Sleep(1000);
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Log.Dispose();
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
