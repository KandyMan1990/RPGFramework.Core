using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    /// <summary>
    /// A settings file from another build: a section in the game's shape, one at another version, one of another size,
    /// one the game no longer writes, and none of a section the game has added.
    /// </summary>
    internal sealed class WhenSavedSettingsAreRead
    {
        private ISettingsService m_Settings;

        [SetUp]
        public void SetUp()
        {
            Dictionary<ulong, SectionBlob> saved = new Dictionary<ulong, SectionBlob>();

            SectionFile.SetSection(saved, FakeDefaultSettings.A, new SaveSection<int>(1, 9));
            SectionFile.SetSection(saved, FakeDefaultSettings.B, new SaveSection<int>(1, 9));
            SectionFile.SetSection(saved, FakeDefaultSettings.C, new SaveSection<int>(1, 9));
            SectionFile.SetSection(saved, "Game.Gone", new SaveSection<byte>(1, 9));

            ISaveStorage storage = new FakeSaveStorage();

            storage.Write(SettingsService.SETTINGS_FILE_NAME, SectionFile.Write(saved));

            m_Settings = new SettingsService(new FakeDefaultSettings(), storage);
        }

        [Test]
        public void ThenTheyAreSaved()
        {
            Assert.IsTrue(m_Settings.IsSaved);
        }

        [Test]
        public void ThenOneInTheGamesShapeIsTaken()
        {
            m_Settings.TryGetSection(FakeDefaultSettings.A, out SaveSection<int> section);

            Assert.AreEqual(9, section.Data);
        }

        [Test]
        public void ThenOneAtAnotherVersionKeepsItsDefault()
        {
            m_Settings.TryGetSection(FakeDefaultSettings.B, out SaveSection<int> section);

            Assert.AreEqual(2u, section.Version);
            Assert.AreEqual(0, section.Data);
        }

        [Test]
        public void ThenOneOfAnotherSizeKeepsItsDefault()
        {
            m_Settings.TryGetSection(FakeDefaultSettings.C, out SaveSection<long> section);

            Assert.AreEqual(0L, section.Data);
        }

        [Test]
        public void ThenOneTheFileLacksKeepsItsDefault()
        {
            m_Settings.TryGetSection(FakeDefaultSettings.D, out SaveSection<byte> section);

            Assert.AreEqual(5, section.Data);
        }

        [Test]
        public void ThenOneTheGameNoLongerWritesIsCarriedThrough()
        {
            bool found = m_Settings.TryGetSection("Game.Gone", out SaveSection<byte> section);

            Assert.IsTrue(found);
            Assert.AreEqual(9, section.Data);
        }
    }
}
