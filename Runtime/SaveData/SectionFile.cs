using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using RPGFramework.Hashing;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// What reading a <see cref="SectionFile" /> found.
    /// </summary>
    internal enum SectionFileStatus
    {
        Intact,

        /// <summary>
        /// Its checksum or its table of contents is wrong — a write cut short, or a file damaged since.
        /// </summary>
        Damaged,

        /// <summary>
        /// Written in a container format this build does not know.
        /// </summary>
        FromNewerVersion
    }

    /// <summary>
    /// The file format saves and settings share: a header — a magic number, the container version and a checksum of
    /// everything after it — then a section count, a table of contents giving each section's id hash, version, offset
    /// and size, then each section's bytes. A section is found by its id, so a section nothing reads is carried through
    /// a rewrite rather than lost. A file without the magic number was written before the header existed, and starts
    /// at its section count.
    /// </summary>
    internal static class SectionFile
    {
        private const uint   MAGIC             = 0x53475052;
        private const ushort CONTAINER_VERSION = 1;
        private const int    HEADER_SIZE       = sizeof(uint) + sizeof(ushort) + sizeof(ulong);
        private const int    TOC_ENTRY_SIZE    = sizeof(ulong) + sizeof(uint) + sizeof(int) + sizeof(int);
        private const string TEMPORARY_SUFFIX  = ".tmp";

        /// <summary>
        /// Every section of the file at <paramref name="path" />, or none if it is not intact.
        /// </summary>
        internal static SectionFileStatus Read(string path, Dictionary<ulong, SectionBlob> sections)
        {
            byte[] bytes = File.ReadAllBytes(path);

            SectionFileStatus status = Parse(bytes, sections);

            if (status != SectionFileStatus.Intact)
            {
                sections.Clear();
            }

            return status;
        }

        /// <summary>
        /// Written to a temporary file and then moved over the old one, so a write cut short leaves the previous file
        /// as it was.
        /// </summary>
        internal static void Write(string path, Dictionary<ulong, SectionBlob> sections)
        {
            byte[] bytes     = Build(sections);
            string temporary = path + TEMPORARY_SUFFIX;

            File.WriteAllBytes(temporary, bytes);

            if (File.Exists(path))
            {
                File.Replace(temporary, path, null);
            }
            else
            {
                File.Move(temporary, path);
            }
        }

        private static byte[] Build(Dictionary<ulong, SectionBlob> sections)
        {
            int length = HEADER_SIZE + sizeof(int) + TOC_ENTRY_SIZE * sections.Count;

            foreach (SectionBlob section in sections.Values)
            {
                length += section.Data.Length;
            }

            byte[] bytes  = new byte[length];
            int    toc    = HEADER_SIZE + sizeof(int);
            int    offset = toc + TOC_ENTRY_SIZE * sections.Count;

            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0), MAGIC);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(sizeof(uint)), CONTAINER_VERSION);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(HEADER_SIZE), sections.Count);

            foreach (KeyValuePair<ulong, SectionBlob> section in sections)
            {
                byte[] data = section.Value.Data;

                BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(toc),                                              section.Key);
                BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(toc + sizeof(ulong)),                              section.Value.Version);
                BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(toc  + sizeof(ulong) + sizeof(uint)),               offset);
                BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(toc  + sizeof(ulong) + sizeof(uint) + sizeof(int)), data.Length);

                Buffer.BlockCopy(data, 0, bytes, offset, data.Length);

                toc    += TOC_ENTRY_SIZE;
                offset += data.Length;
            }

            ulong checksum = Fnv1a64.Hash(new ReadOnlySpan<byte>(bytes, HEADER_SIZE, bytes.Length - HEADER_SIZE));

            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(sizeof(uint) + sizeof(ushort)), checksum);

            return bytes;
        }

        private static SectionFileStatus Parse(byte[] bytes, Dictionary<ulong, SectionBlob> sections)
        {
            int position = 0;

            if (bytes.Length >= sizeof(uint) && BinaryPrimitives.ReadUInt32LittleEndian(bytes) == MAGIC)
            {
                if (bytes.Length < HEADER_SIZE)
                {
                    return SectionFileStatus.Damaged;
                }

                if (BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(sizeof(uint))) > CONTAINER_VERSION)
                {
                    return SectionFileStatus.FromNewerVersion;
                }

                ulong checksum = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(sizeof(uint) + sizeof(ushort)));

                if (Fnv1a64.Hash(new ReadOnlySpan<byte>(bytes, HEADER_SIZE, bytes.Length - HEADER_SIZE)) != checksum)
                {
                    return SectionFileStatus.Damaged;
                }

                position = HEADER_SIZE;
            }

            if (bytes.Length - position < sizeof(int))
            {
                return SectionFileStatus.Damaged;
            }

            int  sectionCount = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(position));
            long tocEnd       = position + sizeof(int) + (long)sectionCount * TOC_ENTRY_SIZE;

            if (sectionCount < 0 || tocEnd > bytes.Length)
            {
                return SectionFileStatus.Damaged;
            }

            for (int toc = position + sizeof(int); toc < tocEnd; toc += TOC_ENTRY_SIZE)
            {
                ulong id      = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(toc));
                uint  version = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(toc + sizeof(ulong)));
                int   offset  = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(toc  + sizeof(ulong) + sizeof(uint)));
                int   size    = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(toc  + sizeof(ulong) + sizeof(uint) + sizeof(int)));

                if (offset < tocEnd || size < 0 || (long)offset + size > bytes.Length)
                {
                    return SectionFileStatus.Damaged;
                }

                sections[id] = new SectionBlob(version, new ReadOnlySpan<byte>(bytes, offset, size).ToArray());
            }

            return SectionFileStatus.Intact;
        }

        internal static bool TryGetSection<T>(Dictionary<ulong, SectionBlob> sections, string sectionId, out SaveSection<T> section) where T : unmanaged
        {
            ulong hash = Fnv1a64.Hash(sectionId);
            if (!sections.TryGetValue(hash, out SectionBlob sectionBlob))
            {
                section = default;
                return false;
            }

            T data = MemoryMarshal.Read<T>(sectionBlob.Data);
            section = new SaveSection<T>(sectionBlob.Version, data);
            return true;
        }

        internal static unsafe void SetSection<T>(Dictionary<ulong, SectionBlob> sections, string sectionId, SaveSection<T> section) where T : unmanaged
        {
            ulong hash = Fnv1a64.Hash(sectionId);

            byte[] bytes = new byte[sizeof(T)];
            MemoryMarshal.Write(bytes, ref section.Data);

            sections[hash] = new SectionBlob(section.Version, bytes);
        }
    }
}