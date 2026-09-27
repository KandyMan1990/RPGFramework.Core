using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using RPGFramework.Hashing;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// The file format saves and settings share: a section count, a table of contents giving each section's id hash,
    /// version, offset and size, then each section's bytes. A section is found by its id, so a section nothing reads is
    /// carried through a rewrite rather than lost.
    /// </summary>
    internal static class SectionFile
    {
        private const int TOC_ENTRY_SIZE = sizeof(ulong) + sizeof(uint) + sizeof(int) + sizeof(int);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct SectionTocEntry
        {
            public readonly ulong SectionId;
            public readonly uint  Version;
            public readonly int   Offset;
            public readonly int   Size;

            public SectionTocEntry(ulong sectionId, uint version, int offset, int size)
            {
                SectionId = sectionId;
                Version   = version;
                Offset    = offset;
                Size      = size;
            }
        }

        internal static void Read(string path, Dictionary<ulong, SectionBlob> sections)
        {
            using FileStream   fs     = File.OpenRead(path);
            using BinaryReader reader = new BinaryReader(fs);

            int sectionCount = reader.ReadInt32();

            SectionTocEntry[] toc = new SectionTocEntry[sectionCount];

            for (int i = 0; i < sectionCount; i++)
            {
                ulong id      = reader.ReadUInt64();
                uint  version = reader.ReadUInt32();
                int   offset  = reader.ReadInt32();
                int   size    = reader.ReadInt32();

                toc[i] = new SectionTocEntry(id, version, offset, size);
            }

            foreach (SectionTocEntry sectionTocEntry in toc)
            {
                fs.Position = sectionTocEntry.Offset;
                byte[] data = reader.ReadBytes(sectionTocEntry.Size);

                sections[sectionTocEntry.SectionId] = new SectionBlob(sectionTocEntry.Version, data);
            }
        }

        internal static void Write(string path, Dictionary<ulong, SectionBlob> sections)
        {
            using FileStream   fs     = File.Create(path);
            using BinaryWriter writer = new BinaryWriter(fs);

            int sectionCount = sections.Count;
            writer.Write(sectionCount);

            long tocStart = fs.Position;

            fs.Position += TOC_ENTRY_SIZE * sectionCount;

            List<SectionTocEntry> toc = new List<SectionTocEntry>(sectionCount);

            foreach (KeyValuePair<ulong, SectionBlob> kvp in sections)
            {
                ulong  id      = kvp.Key;
                uint   version = kvp.Value.Version;
                byte[] data    = kvp.Value.Data;

                int offset = (int)fs.Position;
                writer.Write(data);
                int size = data.Length;

                toc.Add(new SectionTocEntry(id, version, offset, size));
            }

            fs.Position = tocStart;
            foreach (SectionTocEntry entry in toc)
            {
                writer.Write(entry.SectionId);
                writer.Write(entry.Version);
                writer.Write(entry.Offset);
                writer.Write(entry.Size);
            }
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
