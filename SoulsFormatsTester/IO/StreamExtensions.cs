using System.IO;

namespace SoulsFormatsTester.IO
{
    internal static class StreamExtensions
    {
        public static byte GetByte(this Stream stream, long offset)
        {
            long originalPos = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            int read = stream.ReadByte();
            stream.Position = originalPos;

            if (read == -1)
            {
                throw new EndOfStreamException("Cannot read beyond the end of the stream.");
            }

            return (byte)read;
        }

        public static byte[] GetBytes(this Stream stream, long offset, int length)
        {
            long originalPos = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);

            byte[] bytes = new byte[length];
            stream.ReadExactly(bytes, 0, length);
            stream.Position = originalPos;
            return bytes;
        }
    }
}
