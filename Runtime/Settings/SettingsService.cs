using System.Collections.Generic;
using System.IO;
using RPGFramework.Core.SaveData;
using UnityEngine;

namespace RPGFramework.Core.Settings
{
    /// <summary>
    /// The player's settings — language, volumes, message speeds — in a file of their own beside the saves, because
    /// they belong to whoever is at the machine rather than to a playthrough. Sectioned the way a save is, so a game
    /// can keep its own settings beside the framework's.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Whether settings have been written on this machine. Until they are, every section holds the game's
        /// defaults from <see cref="ISettingsFactory" />.
        /// </summary>
        bool IsSaved { get; }

        bool TryGetSection<T>(string sectionId, out SaveSection<T> section) where T : unmanaged;
        void SetSection<T>(string    sectionId, SaveSection<T>     section) where T : unmanaged;

        /// <summary>
        /// Write every section to the settings file.
        /// </summary>
        void Commit();
    }

    /// <summary>
    /// The game's settings on a first launch.
    /// </summary>
    public interface ISettingsFactory
    {
        /// <summary>
        /// Populate the settings with every section the game expects, at its starting values.
        /// </summary>
        void CreateDefaultSettings(ISettingsService settingsService);
    }

    internal sealed class SettingsService : ISettingsService
    {
        private const string SETTINGS_FILE_NAME = "settings.dat";

        private readonly Dictionary<ulong, SectionBlob> m_Sections;
        private readonly ISettingsFactory               m_SettingsFactory;
        private readonly string                         m_Path;

        private bool m_Loaded;
        private bool m_IsSaved;

        public SettingsService(ISettingsFactory settingsFactory)
        {
            m_Sections        = new Dictionary<ulong, SectionBlob>();
            m_SettingsFactory = settingsFactory;
            m_Path            = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);
        }

        bool ISettingsService.IsSaved
        {
            get
            {
                EnsureLoaded();

                return m_IsSaved;
            }
        }

        bool ISettingsService.TryGetSection<T>(string sectionId, out SaveSection<T> section)
        {
            EnsureLoaded();

            bool found = SectionFile.TryGetSection(m_Sections, sectionId, out section);

            return found;
        }

        void ISettingsService.SetSection<T>(string sectionId, SaveSection<T> section)
        {
            EnsureLoaded();

            SectionFile.SetSection(m_Sections, sectionId, section);
        }

        void ISettingsService.Commit()
        {
            EnsureLoaded();

            SectionFile.Write(m_Path, m_Sections);

            m_IsSaved = true;
        }

        private void EnsureLoaded()
        {
            if (m_Loaded)
            {
                return;
            }

            // Set first: the factory fills the defaults in through this service.
            m_Loaded = true;

            if (File.Exists(m_Path))
            {
                SectionFile.Read(m_Path, m_Sections);

                m_IsSaved = true;

                return;
            }

            m_SettingsFactory.CreateDefaultSettings(this);
        }
    }
}
