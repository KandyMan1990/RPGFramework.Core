using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Data;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenSectionsAreWrittenAndReadBack
    {
        private Dictionary<ulong, SectionBlob> m_ReadBack;

        [SetUp]
        public void SetUp()
        {
            SectionFiles.Read(SectionFiles.TwoSections(), out m_ReadBack);
        }

        [Test]
        public void ThenASectionKeepsItsVersionAndData()
        {
            bool found = SectionFile.TryGetSection(m_ReadBack, SectionFiles.CONFIG, out SaveSection<ConfigData_V1> config);

            Assert.IsTrue(found);
            Assert.AreEqual(3u, config.Version);
            Assert.AreEqual("fr-FR", config.Data.GetLanguage());
            Assert.AreEqual(0.25f, config.Data.MusicVolume);
            Assert.AreEqual(0.75f, config.Data.FieldMessageSpeed);
        }

        [Test]
        public void ThenASectionOfAnotherTypeReadsBackBesideIt()
        {
            bool found = SectionFile.TryGetSection(m_ReadBack, SectionFiles.OTHER, out SaveSection<long> other);

            Assert.IsTrue(found);
            Assert.AreEqual(7u, other.Version);
            Assert.AreEqual(0x0102030405060708L, other.Data);
        }

        [Test]
        public void ThenASectionNeverWrittenIsMissing()
        {
            Assert.IsFalse(SectionFile.TryGetSection(m_ReadBack, "Never.Written", out SaveSection<int> _));
        }
    }
}
