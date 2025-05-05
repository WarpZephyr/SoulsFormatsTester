using SoulsFormats;
using SoulsFormats.Dreamcast;
using SoulsFormats.Other;
using SoulsFormatsTester.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace SoulsFormatsTester.Search
{
    internal class SearchEngine
    {
        #region Constants

        private const int BytesSizeThreshold = 1024 * 1024 * 500; // 500MB
        private const int MagicLength = 4;
        private const int FlverEndiannessOffset = 6;
        private const int FlverMajorVersionOffset = 10;

        #endregion

        #region Delegates

        private delegate bool TryGetSplitFileDelegate(string name, string format, [NotNullWhen(true)] out SearchData? data);

        #endregion

        #region Event Handlers

        public EventHandler<SearchDataEventArgs>? OnSearch;

        #endregion

        #region Members

        private readonly HashSet<string> FoundSplitFiles;

        #endregion

        #region Constructors

        public SearchEngine()
        {
            FoundSplitFiles = [];
        }

        #endregion

        #region IO Searches

        public void SearchFolder(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                SearchFile(file);
            }
        }

        public void SearchFile(string path)
        {
            // Make sure we haven't already found this one
            if (FoundSplitFiles.Contains(path))
            {
                return;
            }

            // If the file size is very large use a stream instead
            var fileInfo = new FileInfo(path);
            SearchData data;
            if (fileInfo.Length < BytesSizeThreshold)
            {
                byte[] bytes = File.ReadAllBytes(path);
                data = SearchBytesInternal(path, string.Empty, bytes);
            }
            else
            {
                FileStream fs = fileInfo.OpenRead();
                data = SearchStreamInternal(path, string.Empty, fs, false);
            }

            // We should probably handle feedback searches before passing the search data out
            // Due to the possibility it can be disposed before we get to search it

            // Special handling for Zero3
            // There is no recursive support for it right now
            // It likely doesn't need recursive search anyways
            if (data.Format == "Zero3")
            {
                if (path.EndsWith(".000"))
                {
                    SearchZero3(path);
                }
                else if (path.Length >= 4)
                {
                    string containerPathBase = path[..^3];
                    string containerPath = $"{containerPathBase}{0:D3}";
                    if (File.Exists(containerPath))
                    {
                        SearchZero3(containerPath);
                    }
                }
            }

            SearchFeedbackFormat(data, TryGetSplitFile);
            OnSearch?.Invoke(this, new SearchDataEventArgs(data));
        }

        #endregion

        #region Binder Searches

        private void SearchBND2(string name, string container, BND2Reader reader)
        {
            bool TryGetSplitFile(string name, string format, [NotNullWhen(true)] out SearchData? data)
                => TryGetSplitFileBND2(reader, container, name, format, out data);

            foreach (var file in reader.Files)
            {
                // Make sure we haven't already found this one
                string fileContainer = CombineContainer(container, name);
                if (!FoundSplitFiles.Contains(fileContainer))
                {
                    var data = SearchBytesInternal(file.Name, fileContainer, reader.ReadFile(file));
                    SearchFeedbackFormat(data, TryGetSplitFile);
                    OnSearch?.Invoke(this, new SearchDataEventArgs(data));
                }
            }
        }

        private void SearchBND3(string name, string container, BND3Reader reader)
        {
            bool TryGetSplitFile(string name, string format, [NotNullWhen(true)] out SearchData? data)
                => TryGetSplitFileBND3(reader, container, name, format, out data);

            int index = 0;
            foreach (var file in reader.Files)
            {
                // Make sure we haven't already found this one
                string fileContainer = CombineContainer(container, name);
                if (!FoundSplitFiles.Contains(fileContainer))
                {
                    var data = SearchBytesInternal(GetBinderFileName(file, index), fileContainer, reader.ReadFile(file));
                    SearchFeedbackFormat(data, TryGetSplitFile);
                    OnSearch?.Invoke(this, new SearchDataEventArgs(data));
                }

                index++;
            }
        }

        private void SearchBND4(string name, string container, BND4Reader reader)
        {
            bool TryGetSplitFile(string name, string format, [NotNullWhen(true)] out SearchData? data)
                => TryGetSplitFileBND4(reader, container, name, format, out data);

            int index = 0;
            foreach (var file in reader.Files)
            {
                // Make sure we haven't already found this one
                string fileContainer = CombineContainer(container, name);
                if (!FoundSplitFiles.Contains(fileContainer))
                {
                    var data = SearchBytesInternal(GetBinderFileName(file, index), fileContainer, reader.ReadFile(file));
                    SearchFeedbackFormat(data, TryGetSplitFile);
                    OnSearch?.Invoke(this, new SearchDataEventArgs(data));
                }

                index++;
            }
        }

        private void SearchBXF3(string name, string container, BXF3Reader reader)
        {
            bool TryGetSplitFile(string name, string format, [NotNullWhen(true)] out SearchData? data)
                => TryGetSplitFileBXF3(reader, container, name, format, out data);

            int index = 0;
            foreach (var file in reader.Files)
            {
                // Make sure we haven't already found this one
                string fileContainer = CombineContainer(container, name);
                if (!FoundSplitFiles.Contains(fileContainer))
                {
                    var data = SearchBytesInternal(GetBinderFileName(file, index), fileContainer, reader.ReadFile(file));
                    SearchFeedbackFormat(data, TryGetSplitFile);
                    OnSearch?.Invoke(this, new SearchDataEventArgs(data));
                }

                index++;
            }
        }

        private void SearchBXF4(string name, string container, BXF4Reader reader)
        {
            bool TryGetSplitFile(string name, string format, [NotNullWhen(true)] out SearchData? data)
                => TryGetSplitFileBXF4(reader, container, name, format, out data);

            int index = 0;
            foreach (var file in reader.Files)
            {
                // Make sure we haven't already found this one
                string fileContainer = CombineContainer(container, name);
                if (!FoundSplitFiles.Contains(fileContainer))
                {
                    var data = SearchBytesInternal(GetBinderFileName(file, index), fileContainer, reader.ReadFile(file));
                    SearchFeedbackFormat(data, TryGetSplitFile);
                    OnSearch?.Invoke(this, new SearchDataEventArgs(data));
                }

                index++;
            }
        }

        #endregion

        #region Zero3 Searches

        private void SearchZero3(string path)
        {
            var zero3 = Zero3.Read(path);
            SearchZero3(path, string.Empty, zero3);

            int index = 1;
            string containerPathBase = path[..^3];
            string containerPath = $"{containerPathBase}{index:D3}";
            while (File.Exists(containerPath))
            {
                var containerFileInfo = new FileInfo(containerPath);
                
                // Make sure we haven't already found this one
                if (FoundSplitFiles.Add(containerPath))
                {
                    // If the file size is very large use a stream instead
                    if (containerFileInfo.Length < BytesSizeThreshold)
                    {
                        OnSearch?.Invoke(this, new SearchDataEventArgs(new SearchData(containerPath, string.Empty, "Zero3", File.ReadAllBytes(containerPath))));
                    }
                    else
                    {
                        OnSearch?.Invoke(this, new SearchDataEventArgs(new SearchData(containerPath, string.Empty, "Zero3", containerFileInfo.OpenRead())));
                    }
                }

                index++;
                containerPath = $"{containerPathBase}{index:D3}";
            }
        }

        private void SearchZero3(string name, string container, Zero3 zero3)
        {
            bool TryGetSplitFile(string name, string format, [NotNullWhen(true)] out SearchData? data)
                => TryGetSplitFileZero3(zero3, container, name, format, out data);

            foreach (var file in zero3.Files)
            {
                // Make sure we haven't already found this one
                string fileContainer = CombineContainer(container, name);
                if (!FoundSplitFiles.Contains(fileContainer))
                {
                    var data = SearchBytesInternal(file.Name, fileContainer, file.Bytes);
                    SearchFeedbackFormat(data, TryGetSplitFile);
                    OnSearch?.Invoke(this, new SearchDataEventArgs(data));
                }
            }
        }

        #endregion

        #region DCX Searches

        private void SearchDCX(string name, string container, byte[] decompressed, TryGetSplitFileDelegate tryGetSplitFile)
        {
            // Try to get a non-dcx file name
            string fileName;
            if (name.ToLower().EndsWith(".dcx"))
            {
                fileName = name[..(name.Length - 4)];
            }
            else
            {
                fileName = name;   
            }
            
            // Make sure we haven't already found this one
            string fileContainer = CombineContainer(container, name);
            if (!FoundSplitFiles.Contains(fileContainer))
            {
                SearchData data = SearchBytesInternal(fileName, fileContainer, decompressed);
                SearchFeedbackFormat(data, TryGetSplitFile);
                OnSearch?.Invoke(this, new SearchDataEventArgs(data));
            }
        }

        #endregion

        #region Other Searches

        private void SearchLDMU(string name, string container, LDMU bnd)
        {
            foreach (var file in bnd.Files)
            {
                // Make sure we haven't already found this one
                string fileContainer = CombineContainer(container, name);
                if (!FoundSplitFiles.Contains(fileContainer))
                {
                    var data = SearchBytesInternal(file.ID.ToString(), CombineContainer(container, name), file.Bytes);
                    SearchFeedbackFormat(data, TryGetSplitFile);
                    OnSearch?.Invoke(this, new SearchDataEventArgs(data));
                }
            }
        }

        private void SearchMGF(string name, string container, MGF mgf)
        {
            int index = 0;
            foreach (var file in mgf.Files)
            {
                // Make sure we haven't already found this one
                string fileContainer = CombineContainer(container, name);
                if (!FoundSplitFiles.Contains(fileContainer))
                {
                    var data = SearchBytesInternal(index.ToString(), CombineContainer(container, name), file.Bytes);
                    SearchFeedbackFormat(data, TryGetSplitFile);
                    OnSearch?.Invoke(this, new SearchDataEventArgs(data));
                }

                index++;
            }
        }

        #endregion

        #region Generic Searches

        private static SearchData SearchBytesInternal(string name, string container, byte[] bytes)
        {
            if (bytes.Length < MagicLength)
            {
                // End early if there is nothing to detect
                return new SearchData(name, container, string.Empty, bytes);
            }

            var byteSpan = new Span<byte>(bytes);
            Span<byte> magicBytes = byteSpan[..MagicLength];
            string format = GuessFormat(name, magicBytes);

            // Do additional checks
            if (byteSpan.Length > FlverMajorVersionOffset)
            {
                byte majorVersion;
                if (byteSpan[FlverEndiannessOffset] == 0x42)
                    majorVersion = byteSpan[FlverMajorVersionOffset - 1];
                else
                    majorVersion = byteSpan[FlverMajorVersionOffset];

                format = GuessFlverFormat(format, majorVersion);
            }

            return new SearchData(name, container, format, bytes);
        }

        private static SearchData SearchStreamInternal(string name, string container, Stream stream, bool leaveStreamOpen)
        {
            if (stream.Length < MagicLength)
            {
                // End early if there is nothing to detect
                return new SearchData(name, container, string.Empty, stream, leaveStreamOpen);
            }

            Span<byte> magicBytes = stream.GetBytes(0, MagicLength);
            string format = GuessFormat(name, magicBytes);

            // Do additional checks
            if (stream.Length > FlverMajorVersionOffset)
            {
                byte majorVersion;
                if (stream.GetByte(FlverEndiannessOffset) == 0x42)
                    majorVersion = stream.GetByte(FlverMajorVersionOffset - 1);
                else
                    majorVersion = stream.GetByte(FlverMajorVersionOffset);

                format = GuessFlverFormat(format, majorVersion);
            }

            return new SearchData(name, container, format, stream, leaveStreamOpen);
        }

        #endregion

        #region Search Feedback Format

        private void SearchFeedbackFormat(SearchData data, TryGetSplitFileDelegate tryGetSplitFile)
        {
            switch (data.Format)
            {
                case "BND2":
                    var bnd2reader = ReadBND2(data);
                    SearchBND2(data.Name, data.Container, bnd2reader);
                    break;
                case "BND3":
                    var bnd3reader = ReadBND3(data);
                    SearchBND3(data.Name, data.Container, bnd3reader);
                    break;
                case "BND4":
                    var bnd4reader = ReadBND4(data);
                    SearchBND4(data.Name, data.Container, bnd4reader);
                    break;
                case "MGF":
                    var mgf = ReadMGF(data);
                    SearchMGF(data.Name, data.Container, mgf);
                    break;
                case "DCX":
                    if (IsDCX(data))
                    {
                        var dcx = ReadDCX(data);
                        SearchDCX(data.Name, data.Container, dcx, tryGetSplitFile);
                    }
                    break;
                case "BHF3":
                    if (tryGetSplitFile(GetBdtName(data.Name), "BDF3", out SearchData? bdt3))
                    {
                        var bxf3reader = ReadBXF3(data, bdt3);
                        SearchBXF3(data.Name, data.Container, bxf3reader);
                        ResetStream(bdt3);
                        OnSearch?.Invoke(this, new SearchDataEventArgs(bdt3));
                        FoundSplitFiles.Add(bdt3.Name);
                    }

                    FoundSplitFiles.Add(data.Name);
                    break;
                case "BHF4":
                    if (tryGetSplitFile(GetBdtName(data.Name), "BDF4", out SearchData? bdt4))
                    {
                        var bxf4reader = ReadBXF4(data, bdt4);
                        SearchBXF4(data.Name, data.Container, bxf4reader);
                        ResetStream(bdt4);
                        OnSearch?.Invoke(this, new SearchDataEventArgs(bdt4));
                        FoundSplitFiles.Add(bdt4.Name);
                    }

                    FoundSplitFiles.Add(data.Name);
                    break;
                case "BDF3":
                    if (tryGetSplitFile(GetBhdName(data.Name), "BHF3", out SearchData? bhd3))
                    {
                        var bxf3reader = ReadBXF3(bhd3, data);
                        SearchBXF3(data.Name, data.Container, bxf3reader);
                        ResetStream(bhd3);
                        OnSearch?.Invoke(this, new SearchDataEventArgs(bhd3));
                        FoundSplitFiles.Add(bhd3.Name);
                    }

                    FoundSplitFiles.Add(data.Name);
                    break;
                case "BDF4":
                    if (tryGetSplitFile(GetBhdName(data.Name), "BHF4", out SearchData? bhd4))
                    {
                        var bxf4reader = ReadBXF4(bhd4, data);
                        SearchBXF4(data.Name, data.Container, bxf4reader);
                        ResetStream(bhd4);
                        OnSearch?.Invoke(this, new SearchDataEventArgs(bhd4));
                        FoundSplitFiles.Add(bhd4.Name);
                    }

                    FoundSplitFiles.Add(data.Name);
                    break;
                case "BHD5":
                    // Searching not supported
                    if (tryGetSplitFile(GetBdtName(data.Name), "BHD5", out SearchData? bhd5Data))
                    {
                        OnSearch?.Invoke(this, new SearchDataEventArgs(bhd5Data));
                        FoundSplitFiles.Add(bhd5Data.Name);
                    }
                    else if (tryGetSplitFile(GetBdtName(GetBhd5DataName(data.Name)), "BHD5", out bhd5Data))
                    {
                        OnSearch?.Invoke(this, new SearchDataEventArgs(bhd5Data));
                        FoundSplitFiles.Add(bhd5Data.Name);
                    }

                    FoundSplitFiles.Add(data.Name);
                    break;
                case "LDMU":
                    if (tryGetSplitFile(GetLdmuDataName(data.Name), "LDMU", out SearchData? ldmuData))
                    {
                        var ldmu = ReadLDMU(data, ldmuData);
                        ResetStream(ldmuData);
                        SearchLDMU(data.Name, data.Container, ldmu);
                        OnSearch?.Invoke(this, new SearchDataEventArgs(ldmuData));
                    }

                    FoundSplitFiles.Add(data.Name);
                    break;
            }

            ResetStream(data);
        }

        #endregion

        #region Search Split Format

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetBhd5Name(string path)
            => path.Replace("dvdbnd", "dvdbnd5");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetBhd5DataName(string path)
            => path.Replace("dvdbnd5", "dvdbnd");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetBhdName(string path)
            => path.Replace("bdt", "bhd");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetBdtName(string path)
            => path.Replace("bhd", "bdt");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetLdmuDataName(string path)
            => path.Replace("bhd", "bnd");

        private static bool TryGetSplitFile(string path, string format, [NotNullWhen(true)] out SearchData? splitFile)
        {
            // If the file size is very large use a stream instead
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                if (fileInfo.Length < BytesSizeThreshold)
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    splitFile = new SearchData(path, string.Empty, format, bytes);
                    return true;
                }
                else
                {
                    FileStream fs = fileInfo.OpenRead();
                    splitFile = new SearchData(path, string.Empty, format, fs);
                    return true;
                }
            }

            splitFile = null;
            return false;
        }

        private static bool TryGetSplitFileBND2(BND2Reader bnd, string container, string name, string format, [NotNullWhen(true)] out SearchData? splitFile)
        {
            foreach (var file in bnd.Files)
            {
                if (file.Name.Equals(name))
                {
                    splitFile = new SearchData(name, container, format, bnd.ReadFile(file));
                    return true;
                }
            }

            splitFile = null;
            return false;
        }

        private static bool TryGetSplitFileBND3(BND3Reader bnd, string container, string name, string format, [NotNullWhen(true)] out SearchData? splitFile)
        {
            foreach (var file in bnd.Files)
            {
                if (file.Name.Equals(name))
                {
                    splitFile = new SearchData(name, container, format, bnd.ReadFile(file));
                    return true;
                }
            }

            splitFile = null;
            return false;
        }

        private static bool TryGetSplitFileBND4(BND4Reader bnd, string container, string name, string format, [NotNullWhen(true)] out SearchData? splitFile)
        {
            foreach (var file in bnd.Files)
            {
                if (file.Name.Equals(name))
                {
                    splitFile = new SearchData(name, container, format, bnd.ReadFile(file));
                    return true;
                }
            }

            splitFile = null;
            return false;
        }

        private static bool TryGetSplitFileBXF3(BXF3Reader bnd, string container, string name, string format, [NotNullWhen(true)] out SearchData? splitFile)
        {
            foreach (var file in bnd.Files)
            {
                if (file.Name.Equals(name))
                {
                    splitFile = new SearchData(name, container, format, bnd.ReadFile(file));
                    return true;
                }
            }

            splitFile = null;
            return false;
        }

        private static bool TryGetSplitFileBXF4(BXF4Reader bnd, string container, string name, string format, [NotNullWhen(true)] out SearchData? splitFile)
        {
            foreach (var file in bnd.Files)
            {
                if (file.Name.Equals(name))
                {
                    splitFile = new SearchData(name, container, format, bnd.ReadFile(file));
                    return true;
                }
            }

            splitFile = null;
            return false;
        }

        private static bool TryGetSplitFileZero3(Zero3 zero3, string container, string name, string format, [NotNullWhen(true)] out SearchData? splitFile)
        {
            foreach (var file in zero3.Files)
            {
                if (file.Name.Equals(name))
                {
                    splitFile = new SearchData(name, container, format, file.Bytes);
                    return true;
                }
            }

            splitFile = null;
            return false;
        }

        #endregion

        #region Search Data Readers

        private static void ResetStream(SearchData data)
        {
            if (data.Stream != null && data.Stream.Position != 0)
            {
                data.Stream.Seek(0, SeekOrigin.Begin);
            }
        }

        private static BND2Reader ReadBND2(SearchData data)
        {
            if (data.Bytes != null)
            {
                return new BND2Reader(data.Bytes);
            }
            else if (data.Stream != null)
            {
                return new BND2Reader(data.Stream);
            }
            else
            {
                throw new Exception($"{nameof(SearchData)} had no source data.");
            }
        }

        private static BND3Reader ReadBND3(SearchData data)
        {
            if (data.Bytes != null)
            {
                return new BND3Reader(data.Bytes);
            }
            else if (data.Stream != null)
            {
                return new BND3Reader(data.Stream);
            }
            else
            {
                throw new Exception($"{nameof(SearchData)} had no source data.");
            }
        }

        private static BND4Reader ReadBND4(SearchData data)
        {
            if (data.Bytes != null)
            {
                return new BND4Reader(data.Bytes);
            }
            else if (data.Stream != null)
            {
                return new BND4Reader(data.Stream);
            }
            else
            {
                throw new Exception($"{nameof(SearchData)} had no source data.");
            }
        }

        private static bool IsDCX(SearchData data)
        {
            string magic;
            if (data.Bytes != null)
            {
                if (data.Bytes.Length < MagicLength)
                    return false;
                magic = GetMagic(data.Bytes.AsSpan()[..MagicLength]);
            }
            else if (data.Stream != null)
            {
                if (data.Stream.Length < MagicLength)
                    return false;
                magic = GetMagic(data.Stream.GetBytes(0, MagicLength));
            }
            else
            {
                throw new Exception($"{nameof(SearchData)} had no source data.");
            }

            return magic == "DCX\0" || magic == "DCP\0";
        }

        private static byte[] ReadDCX(SearchData data)
        {
            if (data.Bytes != null)
            {
                return DCX.Decompress(data.Bytes);
            }
            else if (data.Stream != null)
            {
                return DCX.Decompress(data.Stream);
            }
            else
            {
                throw new Exception($"{nameof(SearchData)} had no source data.");
            }
        }

        private static BXF3Reader ReadBXF3(SearchData bhd, SearchData bdt)
        {
            if (bhd.Bytes != null && bdt.Bytes != null)
            {
                return new BXF3Reader(bhd.Bytes, bdt.Bytes);
            }
            else if (bhd.Stream != null && bdt.Stream != null)
            {
                return new BXF3Reader(bhd.Stream, bdt.Stream);
            }
            else if (bhd.Bytes != null && bdt.Stream != null)
            {
                return new BXF3Reader(bhd.Bytes, bdt.Stream);
            }
            else if (bhd.Stream != null && bdt.Bytes != null)
            {
                return new BXF3Reader(bhd.Stream, bdt.Bytes);
            }
            else
            {
                throw new Exception($"A {nameof(SearchData)} had no source data.");
            }
        }

        private static BXF4Reader ReadBXF4(SearchData bhd, SearchData bdt)
        {
            if (bhd.Bytes != null && bdt.Bytes != null)
            {
                return new BXF4Reader(bhd.Bytes, bdt.Bytes);
            }
            else if (bhd.Stream != null && bdt.Stream != null)
            {
                return new BXF4Reader(bhd.Stream, bdt.Stream);
            }
            else if (bhd.Bytes != null && bdt.Stream != null)
            {
                return new BXF4Reader(bhd.Bytes, bdt.Stream);
            }
            else if (bhd.Stream != null && bdt.Bytes != null)
            {
                return new BXF4Reader(bhd.Stream, bdt.Bytes);
            }
            else
            {
                throw new Exception($"A {nameof(SearchData)} had no source data.");
            }
        }

        private static MGF ReadMGF(SearchData data)
        {
            if (data.Bytes != null)
            {
                return MGF.Read(data.Bytes);
            }
            else if (data.Stream != null)
            {
                return MGF.Read(data.Stream);
            }
            else
            {
                throw new Exception($"{nameof(SearchData)} had no source data.");
            }
        }

        private static LDMU ReadLDMU(SearchData bhd, SearchData bdt)
        {
            if (bhd.Bytes != null && bdt.Bytes != null)
            {
                return LDMU.Read(bhd.Bytes, bdt.Bytes);
            }
            else if (bhd.Stream != null && bdt.Stream != null)
            {
                return LDMU.Read(bhd.Stream, bdt.Stream);
            }
            else if (bhd.Bytes != null && bdt.Stream != null)
            {
                return LDMU.Read(bhd.Bytes, bdt.Stream);
            }
            else if (bhd.Stream != null && bdt.Bytes != null)
            {
                return LDMU.Read(bhd.Stream, bdt.Bytes);
            }
            else
            {
                throw new Exception($"A {nameof(SearchData)} had no source data.");
            }
        }

        #endregion

        #region Container

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string CombineContainer(string container, string name)
            => container == string.Empty ? name : $"{container}|{name}";

        #endregion

        #region Binder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetBinderFileName(BinderFileHeader file, int index)
            => file.Name ?? (file.ID == -1 ? index.ToString() : file.ID.ToString());

        #endregion

        #region Format Guessing

        private static string GuessFlverFormat(string format, byte majorVersion)
        {
            if (format == "FLVER")
            {
                if (majorVersion == 2)
                {
                    format = "FLVER2";
                }
                else if (majorVersion < 2)
                {
                    format = "FLVER0";
                }
            }

            return format;
        }

        private static string GuessFormat(string path, ReadOnlySpan<byte> magicBytes)
        {
            // Check Zero3 early
            if (path.EndsWith(".000", StringComparison.InvariantCultureIgnoreCase))
            {
                return "Zero3";
            }

            // Parse a possible known file signature or part of one.
            string magic = GetMagic(magicBytes);
            switch (magic)
            {
                case "BND3":
                case "BND4":
                case "BHF3":
                case "BDF3":
                case "BDF4":
                case "BHD5":
                case "BLI3":
                case "BLI4":
                case "LDMU":
                case "MDL4":
                case "SMD4":
                case "FSB4":
                case "FSB5":
                case "FEV1":
                case "MOSI":
                case "MOSB":
                case "MOWV":
                case "MOGS":
                case "XPR2":
                    return magic;
                case "BHF4":
                    if (path.ToLowerInvariant().EndsWith(".bdt"))
                        return "BHD5";
                    return magic;
                case "DCX\0":
                case "DCP\0":
                    return "DCX";
                case "FLVE":
                    return "FLVER";
                case "TPF\0":
                    return "TPF";
                case "\x1BLua":
                    return "LUAC";
                case "TAE ":
                    return "TAE";
                case "DRB\0":
                case "\0BRD":
                    return "DRB";
                case "MQB ":
                    return "MQB";
                case "DLsE":
                    return "FFXDLSE";
                case "EVD\0":
                    return "EMEVD";
                case "BND\0":
                    return "BND0";
                case "CAP\0":
                case "\0PAC":
                    return "CAP";
                case "\x2ESFB":
                    return "SFB";
                case "\0PSF":
                    return "PARAMSFO";
                case "PAMF":
                    return "PAM";
                case "RIFF":
                    if (path.ToLowerInvariant().EndsWith(".fev"))
                        return "FEV5";
                    return "WAV";
                case "VAGp":
                    return "VAG";
                case "\x89PNG":
                    return "PNG";
                case "\x7F" + "ELF":
                    return "ELF";
                case "FSGX":
                    return "XGS";
                case "KBDS":
                    return "XSB";
                case "DNBW":
                    return "XWB";
                case "MGFL":
                    return "MGF";
            }

            // Check the file name for possible obvious formats
            string lower = path.ToLowerInvariant();
            if (lower.EndsWith(".lc"))
            {
                return "LUAC";
            }
            else if (lower.EndsWith(".fmg.dcx"))
            {
                return "FMG";
            }
            else if (lower.EndsWith(".emtm"))
            {
                return "PARAM";
            }
            else if (lower.EndsWith(".def"))
            {
                return "PARAMDEF";
            }
            else if (lower.EndsWith(".py"))
            {
                return "PYTHON";
            }
            else if (path.EndsWith("EBOOT.BIN"))
            {
                return "ELF";
            }
            else if (path.EndsWith("JPG"))
            {
                return "JPEG";
            }
            else if (lower.EndsWith(".tm2"))
            {
                return "TIM2";
            }
            else if (lower.EndsWith(".7z"))
            {
                return "7zip";
            }
            else if (lower.EndsWith(".ppmtest"))
            {
                return "AP2";
            }
            else if (lower.EndsWith(".blf"))
            {
                return "BLI";
            }
            else if (lower.EndsWith(".mib"))
            {
                return "MOIB";
            }
            else if (lower.EndsWith("acparts.bin"))
            {
                return "AcParts";
            }
            else if (lower.EndsWith("acvparts.bin"))
            {
                return "AcParts5";
            }
            else if (lower.EndsWith("assemmenu.bin") || lower.EndsWith("asm_disp.bin"))
            {
                return "AssemMenu";
            }
            else if (lower.EndsWith("acconflictinfo.bin"))
            {
                return "AcConflictInfo";
            }
            else if (lower.EndsWith("acattachinfo.bin"))
            {
                return "AcAttachInfo";
            }
            else if (lower.EndsWith("accel.bin"))
            {
                return "Accel";
            }
            else if (lower.EndsWith("install.list"))
            {
                return "InstallList";
            }
            else if (lower.EndsWith("md5.fmg"))
            {
                return "RegulationMd5";
            }

            // Just go with it's extension
            string extension = Path.GetExtension(lower);
            if (extension.StartsWith('.'))
            {
                return extension[1..].ToUpperInvariant();
            }

            return extension.ToUpperInvariant();
        }

        private static string GetMagic(ReadOnlySpan<byte> bytes)
        {
            // We are already size checking beforehand
            Debug.Assert(bytes.Length >= MagicLength, $"Data length should not be less than {nameof(MagicLength)}: {MagicLength} here.");

            Span<char> magicSpan = stackalloc char[MagicLength];
            int read = Encoding.ASCII.GetChars(bytes[..MagicLength], magicSpan);
            if (read != MagicLength)
            {
                return string.Empty;
            }

            return magicSpan.ToString();
        }

        #endregion
    }
}
