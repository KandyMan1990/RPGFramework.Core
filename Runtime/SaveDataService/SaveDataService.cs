using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using RPGFramework.Hashing;
using UnityEngine;

namespace RPGFramework.Core.SaveDataService
{
    public interface ISaveDataService
    {
        void     BeginSave(string filename);
        bool     HasSaveLoaded();
        void     CommitSave();
        bool     TryGetSection<T>(string sectionId, out SaveSection<T> section) where T : unmanaged;
        void     SetSection<T>(string    sectionId, SaveSection<T>     section) where T : unmanaged;
        string[] GetListOfSaveFiles();
        string   GetUnusedSaveFileName();
        void     ClearSaveDataFromMemory();
        bool     TryGetLastWrittenSaveFileName(out string filename);
    }

    public class SaveDataService : ISaveDataService
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

        private readonly Dictionary<ulong, SectionBlob> m_Sections;
        private readonly ISaveDataService               m_SaveDataService;

        private string m_CurrentPath;

        public SaveDataService()
        {
            m_Sections        = new Dictionary<ulong, SectionBlob>();
            m_SaveDataService = this;
        }

        void ISaveDataService.BeginSave(string filename)
        {
            m_Sections.Clear();
            m_CurrentPath = Path.Combine(Application.persistentDataPath, filename);

            if (!File.Exists(m_CurrentPath))
            {
                return;
            }

            using FileStream   fs     = File.OpenRead(m_CurrentPath);
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

                m_Sections[sectionTocEntry.SectionId] = new SectionBlob(sectionTocEntry.Version, data);
            }
        }

        bool ISaveDataService.HasSaveLoaded()
        {
            return m_Sections.Count > 0 && m_CurrentPath != string.Empty;
        }

        void ISaveDataService.CommitSave()
        {
            if (string.IsNullOrWhiteSpace(m_CurrentPath))
            {
                throw new InvalidOperationException($"{nameof(ISaveDataService)}::{nameof(ISaveDataService.CommitSave)} Must call {nameof(ISaveDataService.BeginSave)} before CommitSave");
            }

            using FileStream   fs     = File.Create(m_CurrentPath);
            using BinaryWriter writer = new BinaryWriter(fs);

            int sectionCount = m_Sections.Count;
            writer.Write(sectionCount);

            long tocStart = fs.Position;

            fs.Position += TOC_ENTRY_SIZE * sectionCount;

            List<SectionTocEntry> toc = new List<SectionTocEntry>(sectionCount);

            foreach (KeyValuePair<ulong, SectionBlob> kvp in m_Sections)
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

        bool ISaveDataService.TryGetSection<T>(string sectionId, out SaveSection<T> section)
        {
            ulong hash = Fnv1a64.Hash(sectionId);
            if (!m_Sections.TryGetValue(hash, out SectionBlob sectionBlob))
            {
                section = default;
                return false;
            }

            T data = MemoryMarshal.Read<T>(sectionBlob.Data);
            section = new SaveSection<T>(sectionBlob.Version, data);
            return true;
        }

        unsafe void ISaveDataService.SetSection<T>(string sectionId, SaveSection<T> section)
        {
            ulong hash = Fnv1a64.Hash(sectionId);

            byte[] bytes = new byte[sizeof(T)];
            MemoryMarshal.Write(bytes, ref section.Data);

            m_Sections[hash] = new SectionBlob(section.Version, bytes);
        }

        string[] ISaveDataService.GetListOfSaveFiles()
        {
            string path = Application.persistentDataPath;

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            FileInfo[] files = directoryInfo.GetFiles("*.sav");

            string[] filenames = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                filenames[i] = files[i].Name;
            }

            return filenames;
        }

        string ISaveDataService.GetUnusedSaveFileName()
        {
            HashSet<byte> usedIndices = new HashSet<byte>();

            string[] existingFiles = m_SaveDataService.GetListOfSaveFiles();

            foreach (string file in existingFiles)
            {
                if (byte.TryParse(file.AsSpan(4, 3), out byte index))
                {
                    usedIndices.Add(index);
                }
            }

            byte freeIndex = 0;
            while (usedIndices.Contains(freeIndex))
            {
                freeIndex++;
            }

            return $"save{freeIndex:000}.sav";
        }

        void ISaveDataService.ClearSaveDataFromMemory()
        {
            m_Sections.Clear();
            m_CurrentPath = string.Empty;
        }

        bool ISaveDataService.TryGetLastWrittenSaveFileName(out string filename)
        {
            string path = Application.persistentDataPath;

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            FileInfo[] files = directoryInfo.GetFiles("*.sav");
            filename = string.Empty;

            if (files.Length == 0)
            {
                return false;
            }

            DateTime lastAccessedTime = DateTime.MinValue;

            foreach (FileInfo fileInfo in files)
            {
                if (fileInfo.LastWriteTimeUtc > lastAccessedTime)
                {
                    filename = fileInfo.Name;
                }
            }

            return true;
        }
    }
}