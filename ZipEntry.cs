namespace ZipUtility
{
    public class ZipEntry
    {
        public string Path { get; }
        public uint CompressedSize { get; }
        public uint UncompressedSize { get; }

        internal ZipEntry(string path, uint compressedSize, uint uncompressedSize)
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
