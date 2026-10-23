using System;
using System.Collections.Generic;
using System.Globalization;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// The playthrough's saves: beginning one from a file or a new game, writing it, and listing, previewing and
    /// deleting the save files in the player's data folder. A save is the persistent memory bank.
    /// </summary>
    public interface ISaveDataService
    {
        /// <summary>
        /// Begin a playthrough from a save file: a load if the file exists, a new game if it does not.
        /// </summary>
        void BeginSave(string filename);

        /// <summary>
        /// The file the playthrough was begun from or last saved to. A new game's does not exist until it is saved,
        /// and nor does one whose save was deleted while it played.
        /// </summary>
        string GetCurrentSaveFileName();

        /// <summary>
        /// Write the playthrough to a save file, which becomes the current one.
        /// </summary>
        void CommitSave(string filename);

        /// <summary>Deletes the save file.</summary>
        void DeleteSave(string filename);

        /// <summary>The names of the save files in the player's data folder, in no particular order.</summary>
        string[] GetListOfSaveFiles();

        /// <summary>The lowest-numbered save file name no file has yet; throws when every slot is taken.</summary>
        string GetUnusedSaveFileName();

        /// <summary>
        /// What a save slot shows of the file — when it was written, the values read from it, and whether it can be
        /// loaded — without beginning it.
        /// </summary>
        SavePreview ReadPreview(string filename);
    }

    /// <summary>
    /// Whether a save can be loaded, and if not, why.
    /// </summary>
    internal enum SaveState
    {
        Loadable,
        FromNewerVersion,
        Damaged
    }

    internal sealed class SaveDataService : ISaveDataService
#if ENABLE_RPG_FRAMEWORK_DEBUG
        , IDebugSaveDataService
#endif
    {
        private const string SAVE_FILE_PREFIX    = "save";
        private const string SAVE_FILE_EXTENSION = ".sav";
        private const int    SAVE_INDEX_DIGITS   = 3;

        private static readonly int SAVE_FILE_NAME_LENGTH = SAVE_FILE_PREFIX.Length + SAVE_INDEX_DIGITS + SAVE_FILE_EXTENSION.Length;

#if UNITY_EDITOR
        internal static readonly string SAVE_FILE_SEARCH_PATTERN = SAVE_FILE_PREFIX + new string('?', SAVE_INDEX_DIGITS) + SAVE_FILE_EXTENSION;
#endif

        private readonly Dictionary<ulong, SectionBlob> m_Sections;
        private readonly IMemoryBankAccess              m_MemoryBankAccess;
        private readonly IMemoryService                 m_MemoryService;
        private readonly IVariableMap                   m_VariableMap;
        private readonly ISaveStorage                   m_SaveStorage;
        private readonly SaveFileFormat                 m_Format;

        private string m_CurrentFileName;

        public SaveDataService(IMemoryBankAccess memoryBankAccess,
                               IMemoryService    memoryService,
                               IVariableMap      variableMap,
                               ISaveStorage      saveStorage)
        {
            m_Sections         = new Dictionary<ulong, SectionBlob>();
            m_MemoryBankAccess = memoryBankAccess;
            m_MemoryService    = memoryService;
            m_VariableMap      = variableMap;
            m_SaveStorage      = saveStorage;
            m_Format           = new SaveFileFormat(variableMap, memoryBankAccess.PersistentByteCount);
        }

        void ISaveDataService.BeginSave(string filename)
        {
            m_Sections.Clear();
            m_CurrentFileName = filename;

            if (!m_SaveStorage.Exists(filename))
            {
                // A new save. The banks still hold the previous playthrough's state, so clear them
                // rather than letting it leak into this one, then start every variable at its default.
                m_MemoryBankAccess.ClearPersistent();
                m_MemoryBankAccess.ClearSession();

                VariableDefaults.Write(m_MemoryService, m_VariableMap, MemoryBank.Persistent);
                VariableDefaults.Write(m_MemoryService, m_VariableMap, MemoryBank.Session);
                SetLoadedFromSave(false);
                return;
            }

            SaveState state = m_Format.Read(m_SaveStorage.Read(filename), m_Sections, out byte[] bank);

            if (state != SaveState.Loadable)
            {
                throw new InvalidOperationException($"{nameof(SaveDataService)}::{nameof(ISaveDataService.BeginSave)} {filename} cannot be loaded ({state}). Offer only saves whose {nameof(SavePreview)}.{nameof(SavePreview.CanLoad)} is true");
            }

            Restore(bank, true);
        }

        string ISaveDataService.GetCurrentSaveFileName()
        {
            string filename = m_CurrentFileName;

            return filename;
        }

        void ISaveDataService.CommitSave(string filename)
        {
            m_CurrentFileName = filename;

            m_SaveStorage.Write(filename, m_Format.Write(m_Sections, m_MemoryBankAccess.CopyPersistent()));
        }

        void ISaveDataService.DeleteSave(string filename)
        {
            m_SaveStorage.Delete(filename);
        }

        SavePreview ISaveDataService.ReadPreview(string filename)
        {
            SaveState state = m_Format.Read(m_SaveStorage.Read(filename), new Dictionary<ulong, SectionBlob>(), out byte[] bank);

            SavePreview preview = new SavePreview(filename, m_SaveStorage.GetLastWriteTime(filename), bank, m_VariableMap, state);

            return preview;
        }

        string[] ISaveDataService.GetListOfSaveFiles()
        {
            string[] candidates = m_SaveStorage.GetFileNames();
            int      count      = 0;

            for (int i = 0; i < candidates.Length; i++)
            {
                if (TryGetSaveSlotIndex(candidates[i], out byte _))
                {
                    count++;
                }
            }

            string[] filenames = new string[count];
            int      next      = 0;

            for (int i = 0; i < candidates.Length; i++)
            {
                if (TryGetSaveSlotIndex(candidates[i], out byte _))
                {
                    filenames[next++] = candidates[i];
                }
            }

            return filenames;
        }

#if ENABLE_RPG_FRAMEWORK_DEBUG
        byte[] IDebugSaveDataService.CaptureSave()
        {
            return m_Format.Write(m_Sections, m_MemoryBankAccess.CopyPersistent());
        }

        // Arriving as a new game does, so a module enters by its own way in rather than where a save was made.
        bool IDebugSaveDataService.TryBeginSave(byte[] save)
        {
            Dictionary<ulong, SectionBlob> sections = new Dictionary<ulong, SectionBlob>();

            if (m_Format.Read(save, sections, out byte[] bank) != SaveState.Loadable)
            {
                return false;
            }

            m_Sections.Clear();

            foreach (KeyValuePair<ulong, SectionBlob> section in sections)
            {
                m_Sections[section.Key] = section.Value;
            }

            m_CurrentFileName = UnusedSaveFileName();

            Restore(bank, false);

            return true;
        }
#endif

        string ISaveDataService.GetUnusedSaveFileName()
        {
            return UnusedSaveFileName();
        }

        private string UnusedSaveFileName()
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

        private HashSet<byte> GetUsedSaveSlotIndices()
        {
            HashSet<byte> usedIndices = new HashSet<byte>();

            string[] files = m_SaveStorage.GetFileNames();

            for (int i = 0; i < files.Length; i++)
            {
                if (TryGetSaveSlotIndex(files[i], out byte index))
                {
                    usedIndices.Add(index);
                }
            }

            return usedIndices;
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

        private void SetLoadedFromSave(bool loaded)
        {
            m_VariableMap.TryGetVariable(CoreVariables.LOADED_FROM_SAVE, out VariableDefinition variable);

            m_MemoryService.WriteBool(MemoryBank.Session, (ushort)variable.Offset, loaded);
        }

        // Session state is what survives a module change but not a restart, and beginning from a save is a restart —
        // NPC positions and "have I already heard this line" belong to the playthrough being left, not the one entered.
        private void Restore(byte[] bank, bool loadedFromSave)
        {
            m_MemoryBankAccess.ClearSession();
            VariableDefaults.Write(m_MemoryService, m_VariableMap, MemoryBank.Session);
            SetLoadedFromSave(loadedFromSave);

            m_MemoryBankAccess.RestorePersistent(bank);
        }
    }
}