namespace SoulsFormatsTester.Search
{
    internal record SearchResult
    {
        public string Format { get; init; }
        public bool IsSplitFormat { get; init; }

        public SearchResult(string format, bool isSplitFormat)
        {
            Format = format;
            IsSplitFormat = isSplitFormat;
        }
    }
}
