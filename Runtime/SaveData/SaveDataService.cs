using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RPGFramework.Core.Data;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Hashing;
using UnityEngine;

namespace RPGFramework.Core.SaveData
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

    internal sealed class SaveDataService : ISaveDataService
    {
        private const string SAVE_FILE_PREFIX    = "save";
        private const string SAVE_FILE_EXTENSION = ".sav";
        private const int    SAVE_INDEX_DIGITS   = 3;

        private static readonly int    SAVE_FILE_NAME_LENGTH    = SAVE_FILE_PREFIX.Length + SAVE_INDEX_DIGITS                  + SAVE_FILE_EXTENSION.Length;
        private static readonly string SAVE_FILE_SEARCH_PATTERN = SAVE_FILE_PREFIX        + new string('?', SAVE_INDEX_DIGITS) + SAVE_FILE_EXTENSION;

        private readonly Dictionary<ulong, SectionBlob> m_Sections;
        private readonly IMemoryBankAccess              m_MemoryBankAccess;
        private readonly IMemoryService                 m_MemoryService;
        private readonly IVariableMap                   m_VariableMap;
        private readonly ulong                          m_PersistentMemorySectionId;

        private string m_CurrentPath;

        public SaveDataService(IMemoryBankAccess memoryBankAccess,
                               IMemoryService    memoryService,
                               IVariableMap      variableMap)
        {
            m_Sections                  = new Dictionary<ulong, SectionBlob>();
            m_MemoryBankAccess          = memoryBankAccess;
            m_MemoryService             = memoryService;
            m_VariableMap               = variableMap;
            m_PersistentMemorySectionId = Fnv1a64.Hash(FrameworkSaveSectionDatabase.PERSISTENT_MEMORY);
        }

        void ISaveDataService.BeginSave(string filename)
        {
            m_Sections.Clear();
            m_CurrentPath = Path.Combine(Application.persistentDataPath, filename);

            if (!File.Exists(m_CurrentPath))
            {
                // A new save. The banks still hold the previous playthrough's state, so clear them
                // rather than letting it leak into this one, then start every variable at its default.
                m_MemoryBankAccess.ClearPersistent();
                m_MemoryBankAccess.ClearSession();

                VariableDefaults.Write(m_MemoryService, m_VariableMap, MemoryBank.Persistent, 0);
                VariableDefaults.Write(m_MemoryService, m_VariableMap, MemoryBank.Session,    0);
                SetLoadedFromSave(false);
                return;
            }

            SectionFile.Read(m_CurrentPath, m_Sections);

            // Session state is what survives a module change but not a restart, and loading a save is a
            // restart — NPC positions and "have I already heard this line" belong to the playthrough
            // being left, not the one being entered.
            m_MemoryBankAccess.ClearSession();
            VariableDefaults.Write(m_MemoryService, m_VariableMap, MemoryBank.Session, 0);
            SetLoadedFromSave(true);

            int restoredBytes = RestorePersistentMemory();

            // Variables added since this save was written lie past the end of what it holds.
            VariableDefaults.Write(m_MemoryService, m_VariableMap, MemoryBank.Persistent, restoredBytes);
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

            CapturePersistentMemory();

            SectionFile.Write(m_CurrentPath, m_Sections);
        }

        bool ISaveDataService.TryGetSection<T>(string sectionId, out SaveSection<T> section)
        {
            bool found = SectionFile.TryGetSection(m_Sections, sectionId, out section);

            return found;
        }

        void ISaveDataService.SetSection<T>(string sectionId, SaveSection<T> section)
        {
            SectionFile.SetSection(m_Sections, sectionId, section);
        }

        string[] ISaveDataService.GetListOfSaveFiles()
        {
            List<FileInfo> saveFiles = GetSaveFiles();

            string[] filenames = new string[saveFiles.Count];

            for (int i = 0; i < saveFiles.Count; i++)
            {
                filenames[i] = saveFiles[i].Name;
            }

            return filenames;
        }

        string ISaveDataService.GetUnusedSaveFileName()
        {
            HashSet<byte> usedIndices = GetUsedSaveSlotIndices();

            for (int index = 0; index <= byte.MaxValue; index++)
            {
                if (usedIndices.Contains((byte)index))
                {
                    continue;
                }

                string filename = BuildSaveFileName((byte)index);

                return filename;
            }

            throw new InvalidOperationException($"{nameof(SaveDataService)}::{nameof(ISaveDataService.GetUnusedSaveFileName)} all {byte.MaxValue + 1} save slots are in use, so there is no unused name to give out. Delete a save first");
        }

        private static string BuildSaveFileName(byte index)
        {
            string filename = SAVE_FILE_PREFIX + index.ToString("D" + SAVE_INDEX_DIGITS, CultureInfo.InvariantCulture) + SAVE_FILE_EXTENSION;

            return filename;
        }

        private static HashSet<byte> GetUsedSaveSlotIndices()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath);

            HashSet<byte> usedIndices = new HashSet<byte>();

            foreach (FileInfo file in directoryInfo.GetFiles(SAVE_FILE_SEARCH_PATTERN))
            {
                if (TryGetSaveSlotIndex(file.Name, out byte index))
                {
                    usedIndices.Add(index);
                }
            }

            return usedIndices;
        }

        private static List<FileInfo> GetSaveFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath);

            FileInfo[] candidates = directoryInfo.GetFiles(SAVE_FILE_SEARCH_PATTERN);

            List<FileInfo> saveFiles = new List<FileInfo>(candidates.Length);

            foreach (FileInfo candidate in candidates)
            {
                if (TryGetSaveSlotIndex(candidate.Name, out byte _))
                {
                    saveFiles.Add(candidate);
                }
            }

            return saveFiles;
        }

        private static bool TryGetSaveSlotIndex(string filename, out byte index)
        {
            index = 0;

            if (filename.Length != SAVE_FILE_NAME_LENGTH)
            {
                return false;
            }

            if (!filename.StartsWith(SAVE_FILE_PREFIX, StringComparison.Ordinal))
            {
                return false;
            }

            if (!filename.EndsWith(SAVE_FILE_EXTENSION, StringComparison.Ordinal))
            {
                return false;
            }

            bool parsed = byte.TryParse(filename.AsSpan(SAVE_FILE_PREFIX.Length, SAVE_INDEX_DIGITS),
                                        NumberStyles.None, CultureInfo.InvariantCulture, out index);

            return parsed;
        }

        void ISaveDataService.ClearSaveDataFromMemory()
        {
            m_Sections.Clear();
            m_CurrentPath = string.Empty;

            m_MemoryBankAccess.ClearPersistent();
            m_MemoryBankAccess.ClearSession();
        }

        private void SetLoadedFromSave(bool loaded)
        {
            m_VariableMap.TryGetVariable(CoreVariables.LOADED_FROM_SAVE, out VariableDefinition variable);

            m_MemoryService.WriteBool(MemoryBank.Session, (ushort)variable.Offset, loaded);
        }

        /// <summary>
        /// Copy the persistent memory bank into its reserved section, so <see cref="ISaveDataService.CommitSave" />
        /// writes the variables as they stand right now. The bank is a raw blob rather than a
        /// <see cref="SaveSection{T}" /> because its length is decided by the variable map at build time,
        /// not by an unmanaged struct.
        /// </summary>
        private void CapturePersistentMemory()
        {
            byte[] persistent = m_MemoryBankAccess.CopyPersistent();

            m_Sections[m_PersistentMemorySectionId] = new SectionBlob(Versions.PERSISTENT_MEMORY, persistent);
        }

        /// <summary>
        /// Push the loaded persistent memory section back into the bank. A save written before any variables
        /// existed has no such section, in which case the bank is cleared — the same state a new game gets.
        /// </summary>
        /// <returns>How many bytes of the bank came from the save; everything after them did not.</returns>
        private int RestorePersistentMemory()
        {
            if (!m_Sections.TryGetValue(m_PersistentMemorySectionId, out SectionBlob persistentMemory))
            {
                m_MemoryBankAccess.ClearPersistent();
                return 0;
            }

            m_MemoryBankAccess.RestorePersistent(persistentMemory.Data);

            int restoredBytes = persistentMemory.Data.Length;

            return restoredBytes;
        }

        bool ISaveDataService.TryGetLastWrittenSaveFileName(out string filename)
        {
            List<FileInfo> files = GetSaveFiles();

            filename = string.Empty;

            if (files.Count == 0)
            {
                return false;
            }

            DateTime lastAccessedTime = DateTime.MinValue;

            foreach (FileInfo fileInfo in files)
            {
                if (fileInfo.LastWriteTimeUtc > lastAccessedTime)
                {
                    lastAccessedTime = fileInfo.LastWriteTimeUtc;
                    filename         = fileInfo.Name;
                }
            }

            return true;
        }
    }
}