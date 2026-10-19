using System.Collections.Generic;
using RPGFramework.Core.Data;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal static class SectionFiles
    {
        internal const string CONFIG = "RPGFramework.ConfigData";
        internal const string OTHER  = "Game.Other";

        /// <summary>A file of the framework's config section beside a game's own of another type.</summary>
        internal static byte[] TwoSections()
        {
            ConfigData_V1 config = new ConfigData_V1 { MusicVolume = 0.25f, FieldMessageSpeed = 0.75f };

            config.SetLanguage("fr-FR");

            Dictionary<ulong, SectionBlob> sections = new Dictionary<ulong, SectionBlob>();

            SectionFile.SetSection(sections, CONFIG, new SaveSection<ConfigData_V1>(3, config));
            SectionFile.SetSection(sections, OTHER, new SaveSection<long>(7, 0x0102030405060708L));

            byte[] file = SectionFile.Write(sections);

            return file;
        }

        internal static SectionFileStatus Read(byte[] file, out Dictionary<ulong, SectionBlob> sections)
        {
            sections = new Dictionary<ulong, SectionBlob>();

            SectionFileStatus status = SectionFile.Read(file, sections);

            return status;
        }
    }
}
