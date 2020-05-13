using System.IO;

namespace ZipUtility
{
    public class ZipEntry
    {
        public string Path { get; }
        public long CompressedSize { get; }
        public long UncompressedSize { get; }

        internal ZipEntry(string path, long compressedSize, long uncompressedSize)
        {
            Path = path;
            CompressedSize = compressedSize;
            UncompressedSize = uncompressedSize;
        }
        
        public override string ToString()
        {
            return Path;
        }
    }
}
