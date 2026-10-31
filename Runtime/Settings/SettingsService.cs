using System.Collections.Generic;
using RPGFramework.Core.Data;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Settings
{
    /// <summary>
    /// The player's settings file, beside the saves, because the settings belong to whoever is at the machine rather
    /// than to a playthrough. It holds <see cref="MemoryBank.Settings" /> — Core's settings, read through
    /// <see cref="Store.ISettingsStore" />, and any the game declares in its variable map — read into the bank as the
    /// game starts, laid out as the map lays it out now.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Write the settings bank to the settings file.
        /// </summary>
        void Commit();
    }

    /// <summary>
    /// Reads the settings file into the settings bank, once, as Core starts and before anything reads a setting.
    /// </summary>
    internal interface ISettingsLoader
    {
        void Load();
    }

    internal sealed class SettingsService : ISettingsService, ISettingsLoader
    {
        internal const string SETTINGS_FILE_NAME = "settings.dat";

        private readonly Dictionary<ulong, SectionBlob> m_Sections;
        private readonly IMemoryBankAccess              m_MemoryBankAccess;
        private readonly ISaveStorage                   m_SaveStorage;
        private readonly BankFileFormat                 m_Format;

        public SettingsService(IMemoryBankAccess memoryBankAccess, IVariableMap variableMap, ISaveStorage saveStorage)
        {
            m_Sections         = new Dictionary<ulong, SectionBlob>();
            m_MemoryBankAccess = memoryBankAccess;
            m_SaveStorage      = saveStorage;
            m_Format           = new BankFileFormat(variableMap, MemoryBank.Settings, memoryBankAccess.GetByteCount(MemoryBank.Settings),
                                                    FrameworkSettingsSections.SETTINGS_MEMORY, FrameworkSettingsSections.SETTINGS_MEMORY_LAYOUT);
        }

        void ISettingsService.Commit()
        {
            m_SaveStorage.Write(SETTINGS_FILE_NAME, m_Format.Write(m_Sections, m_MemoryBankAccess.Copy(MemoryBank.Settings)));
        }

        // Whatever could be read is taken: a damaged file gives the defaults, and one from a newer build what this build
        // knows of it. Unlike a save, there is no playthrough to protect by refusing it.
        void ISettingsLoader.Load()
        {
            m_Sections.Clear();

            byte[] bank = m_Format.DefaultBank();

            if (m_SaveStorage.Exists(SETTINGS_FILE_NAME))
            {
                m_Format.Read(m_SaveStorage.Read(SETTINGS_FILE_NAME), m_Sections, out bank);
            }

            m_MemoryBankAccess.Restore(MemoryBank.Settings, bank);
        }
    }
}
