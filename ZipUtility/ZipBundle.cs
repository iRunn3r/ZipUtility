using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace ZipUtility
{
    internal enum Markers
    {
        EntryCount = 10,
        CentralDirectoryRecord = 16,
        CompressedSize = 20,
        UncompressedSize = 24,
        NameLength = 28,
        ExtraLength = 30,
        CommentLength = 32,
        Name = 46
    }

    public class ZipBundle : IDisposable
    {
        public string FullName { get; }
        public long Length { get; }
        public List<ZipEntry> Entries { get; }

        private const int k_EndOfCentralDirectoryMarker = 101010256;
        private readonly FileStream m_ZipStream;
        private long m_EndOfCentralDirectoryOffset;
        private long EndOfCentralDirectoryOffset
        {
            get
            {
                if (m_EndOfCentralDirectoryOffset == 0)
                    m_EndOfCentralDirectoryOffset = FindEndOfCentralDirectoryOffset();
                return m_EndOfCentralDirectoryOffset;
            }
        }

        private long FindEndOfCentralDirectoryOffset()
        {
            var marker = BitConverter.GetBytes(k_EndOfCentralDirectoryMarker);
            for (var offset = 4; offset <= m_ZipStream.Length; offset++)
            {
                m_ZipStream.Seek(-offset, SeekOrigin.End);
                var sizeBytes = new byte[4];
                m_ZipStream.Read(sizeBytes, 0, 4);
                if (!sizeBytes.SequenceEqual(marker))
                    continue;
                m_ZipStream.Seek(-4, SeekOrigin.Current);
                return m_ZipStream.Position;
            }

            throw new Exception("Archive invalid - could not find end-of-central-directory marker.");
        }

        private int GetArchiveEntryCount()
        {
            SeekFromStart(EndOfCentralDirectoryOffset + (int)Markers.EntryCount);
            return ReadUShort();
        }

        private long GetCentralDirectoryOffset()
        {
            SeekFromStart(EndOfCentralDirectoryOffset + (int)Markers.CentralDirectoryRecord);
            return ReadUInt();
        }

        private byte[] GetBytes(int length)
        {
            var resultBytes = new byte[length];
            m_ZipStream.Read(resultBytes, 0, length);
            return resultBytes;
        }

        private ushort ReadUShort()
        {
            return BitConverter.ToUInt16(GetBytes(2), 0);
        }

        private uint ReadUInt()
        {
            return BitConverter.ToUInt32(GetBytes(4), 0);
        }

        private string ReadString(int length)
        {
            return System.Text.Encoding.Default.GetString(GetBytes(length));
        }

        private void ValidateZip()
        {
            FindEndOfCentralDirectoryOffset();
        }

        private void SeekFromStart(long offset)
        {
            m_ZipStream.Seek(offset, SeekOrigin.Begin);
        }

        public ZipBundle(string path)
        {
            m_ZipStream = File.Open(path, FileMode.Open);

            ValidateZip();

            FullName = path;
            Length = new FileInfo(path).Length;

            var entryCount = GetArchiveEntryCount();
            var recordStart = GetCentralDirectoryOffset();
            SeekFromStart(recordStart);
            Entries = new List<ZipEntry>();
            for (var _ = 0; _ < entryCount; _++)
            {
                SeekFromStart(recordStart + (int)Markers.CompressedSize);
                var compressedSize = ReadUInt();

                SeekFromStart(recordStart + (int)Markers.UncompressedSize);
                var uncompressedSize = ReadUInt();

                SeekFromStart(recordStart + (int)Markers.NameLength);
                var nameLength = ReadUShort();

                SeekFromStart(recordStart + (int)Markers.ExtraLength);
                var extraLength = ReadUInt();

                SeekFromStart(recordStart + (int)Markers.CommentLength);
                var commentLength = ReadUShort();

                SeekFromStart(recordStart + (int)Markers.Name);
                var name = ReadString(nameLength);

                Entries.Add(new ZipEntry(name, compressedSize, uncompressedSize));

                recordStart = recordStart + (int) Markers.Name + nameLength + extraLength + commentLength;
            }
        }

        public override string ToString()
        {
            return FullName;
        }

        public void Dispose()
        {
            m_ZipStream.Close();
        }
    }
}
