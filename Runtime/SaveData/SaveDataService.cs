using System;
using System.Collections.Generic;
using System.Globalization;
using RPGFramework.Core.Data;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Hashing;

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
        private readonly ulong                          m_PersistentMemorySectionId;
        private readonly ulong                          m_PersistentLayoutSectionId;
        private readonly byte[]                         m_PersistentLayout;

        private string m_CurrentFileName;

        public SaveDataService(IMemoryBankAccess memoryBankAccess,
                               IMemoryService    memoryService,
                               IVariableMap      variableMap,
                               ISaveStorage      saveStorage)
        {
            m_Sections                  = new Dictionary<ulong, SectionBlob>();
            m_MemoryBankAccess          = memoryBankAccess;
            m_MemoryService             = memoryService;
            m_VariableMap               = variableMap;
            m_SaveStorage               = saveStorage;
            m_PersistentMemorySectionId = Fnv1a64.Hash(FrameworkSaveSections.PERSISTENT_MEMORY);
            m_PersistentLayoutSectionId = Fnv1a64.Hash(FrameworkSaveSections.PERSISTENT_MEMORY_LAYOUT);
            m_PersistentLayout          = PersistentLayout.Write(variableMap);
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

            SaveState state = ReadSave(filename, m_Sections, out byte[] bank);

            if (state != SaveState.Loadable)
            {
                throw new InvalidOperationException($"{nameof(SaveDataService)}::{nameof(ISaveDataService.BeginSave)} {filename} cannot be loaded ({state}). Offer only saves whose {nameof(SavePreview)}.{nameof(SavePreview.CanLoad)} is true");
            }

            // Session state is what survives a module change but not a restart, and loading a save is a
            // restart — NPC positions and "have I already heard this line" belong to the playthrough
            // being left, not the one being entered.
            m_MemoryBankAccess.ClearSession();
            VariableDefaults.Write(m_MemoryService, m_VariableMap, MemoryBank.Session);
            SetLoadedFromSave(true);

            m_MemoryBankAccess.RestorePersistent(bank);
        }

        string ISaveDataService.GetCurrentSaveFileName()
        {
            string filename = m_CurrentFileName;

            return filename;
        }

        void ISaveDataService.CommitSave(string filename)
        {
            m_CurrentFileName = filename;

            CapturePersistentMemory();

            m_SaveStorage.Write(filename, SectionFile.Write(m_Sections));
        }

        void ISaveDataService.DeleteSave(string filename)
        {
            m_SaveStorage.Delete(filename);
        }

        SavePreview ISaveDataService.ReadPreview(string filename)
        {
            SaveState state = ReadSave(filename, new Dictionary<ulong, SectionBlob>(), out byte[] bank);

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

        /// <summary>
        /// Copy the persistent memory bank into its reserved section, so <see cref="ISaveDataService.CommitSave" />
        /// writes the variables as they stand right now, with the layout that says where each one is. The bank is a raw
        /// blob rather than a <see cref="SaveSection{T}" /> because its length is decided by the variable map at build
        /// time, not by an unmanaged struct.
        /// </summary>
        private void CapturePersistentMemory()
        {
            byte[] persistent = m_MemoryBankAccess.CopyPersistent();

            m_Sections[m_PersistentMemorySectionId] = new SectionBlob(Versions.PERSISTENT_MEMORY,      persistent);
            m_Sections[m_PersistentLayoutSectionId] = new SectionBlob(PersistentLayout.FORMAT_VERSION, m_PersistentLayout);
        }

        /// <summary>
        /// A save's sections, and its persistent bank laid out as this build's map lays it out. A save that is damaged
        /// or from a newer version cannot be loaded, and its bank holds the map's defaults, so a preview of it still
        /// reads.
        /// </summary>
        private SaveState ReadSave(string filename, Dictionary<ulong, SectionBlob> sections, out byte[] bank)
        {
            SectionFileStatus file = SectionFile.Read(m_SaveStorage.Read(filename), sections);

            if (file != SectionFileStatus.Intact)
            {
                bank = DefaultBank();

                SaveState unreadable = file == SectionFileStatus.FromNewerVersion ? SaveState.FromNewerVersion : SaveState.Damaged;

                return unreadable;
            }

            // A save written before any variables existed has no persistent memory, so every variable reads its
            // default.
            byte[] persistent = sections.TryGetValue(m_PersistentMemorySectionId, out SectionBlob memory) ? memory.Data : Array.Empty<byte>();
            byte[] layout     = sections.TryGetValue(m_PersistentLayoutSectionId, out SectionBlob saved) ? saved.Data : null;

            SaveState state = ToCurrentLayout(persistent, layout, out bank);

            return state;
        }

        /// <summary>
        /// As saved when the layouts match, otherwise moved value by value. A save with no layout was written before
        /// saves recorded one, when the map could only ever grow at the end, so it is read by the map's own layout as
        /// far as its bytes reach. A layout that cannot be read, or a bank shorter than its layout describes, is
        /// damage.
        /// </summary>
        private SaveState ToCurrentLayout(byte[] persistent, byte[] layout, out byte[] bank)
        {
            if (layout == null)
            {
                bank = PersistentMigration.Migrate(persistent, PersistentLayout.Within(m_VariableMap, persistent.Length), m_VariableMap, m_MemoryBankAccess.PersistentByteCount);

                return SaveState.Loadable;
            }

            if (!PersistentLayout.TryRead(layout, out PersistentLayout savedLayout) || persistent.Length < savedLayout.RequiredBytes)
            {
                bank = DefaultBank();

                return SaveState.Damaged;
            }

            if (layout.AsSpan().SequenceEqual(m_PersistentLayout))
            {
                bank = persistent;

                return SaveState.Loadable;
            }

            bank = PersistentMigration.Migrate(persistent, savedLayout, m_VariableMap, m_MemoryBankAccess.PersistentByteCount);

            SaveState state = PersistentMigration.IsFromNewerBuild(savedLayout, m_VariableMap) ? SaveState.FromNewerVersion : SaveState.Loadable;

            return state;
        }

        private byte[] DefaultBank()
        {
            byte[] bank = new byte[m_MemoryBankAccess.PersistentByteCount];

            VariableDefaults.Write(bank, m_VariableMap, MemoryBank.Persistent);

            return bank;
        }
    }
}