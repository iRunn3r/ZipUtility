using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZipUtility
{
    public class ZipBundle
    {
        public string Path { get; }
        public long Length { get; }
        public List<ZipEntry> Entries { get; }

        public ZipBundle(string path)
        {
            // add a check for these
            Path = path;
            Length = new FileInfo(path).Length;
            var eocdMarker = BitConverter.GetBytes(101010256);
            using (var stream = File.Open(path, FileMode.Open))
            {
                // Seek to beginning of "end of central directory record" (EOCD)
                for (var offset = 4; offset <= stream.Length; offset++)
                {
                    stream.Seek(-offset, SeekOrigin.End);
                    var sizeBytes = new byte[4];
                    stream.Read(sizeBytes, 0, 4);
                    if (!sizeBytes.SequenceEqual(eocdMarker))
                        continue;
                    stream.Seek(-4, SeekOrigin.Current);
                    break;

                }

                // Get number of archive entries
                stream.Seek(10, SeekOrigin.Current);
                var entryCountBytes = new byte[2];
                stream.Read(entryCountBytes, 0, 2);
                var cdRecordCount = BitConverter.ToInt16(entryCountBytes, 0);

                // Seek to CDR
                stream.Seek(4, SeekOrigin.Current);
                var cdrOffsetBytes = new byte[4];
                stream.Read(cdrOffsetBytes, 0, 4);
                var cdrOffset = BitConverter.ToInt32(cdrOffsetBytes, 0);
                stream.Seek(cdrOffset, SeekOrigin.Begin);
                
                Entries = new List<ZipEntry>(cdRecordCount);
                for (var _ = 0; _ < cdRecordCount; _++)
                {
                    stream.Seek(20, SeekOrigin.Current);
                    var compressedSizeBytes = new byte[4];
                    stream.Read(compressedSizeBytes, 0, 4);
                    var compressedSize = BitConverter.ToInt32(compressedSizeBytes, 0);
                    var uncompressedSizeBytes = new byte[4];
                    stream.Read(uncompressedSizeBytes, 0, 4);
                    var uncompressedSize = BitConverter.ToInt32(uncompressedSizeBytes, 0);
                    var nameLengthBytes = new byte[2];
                    stream.Read(nameLengthBytes, 0, 2);
                    var nameLength = BitConverter.ToInt16(nameLengthBytes, 0);
                    var extraLengthBytes = new byte[2];
                    stream.Read(extraLengthBytes, 0, 2);
                    var extraLength = BitConverter.ToInt16(extraLengthBytes, 0);
                    var commentLengthBytes = new byte[2];
                    stream.Read(commentLengthBytes, 0, 2);
                    var commentLength = BitConverter.ToInt16(commentLengthBytes, 0);
                    stream.Seek(12, SeekOrigin.Current);
                    var nameBytes = new byte[nameLength];
                    stream.Read(nameBytes, 0, nameLength);
                    var entryName = System.Text.Encoding.Default.GetString(nameBytes);
                    stream.Seek(extraLength + commentLength, SeekOrigin.Current);

                    Entries.Add(new ZipEntry(entryName, compressedSize, uncompressedSize));
                }
            }
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
