using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.SaveData;
using RPGFramework.Hashing;

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
            bool found = m_ReadBack.TryGetValue(Fnv1a64.Hash(SectionFiles.FIRST), out SectionBlob first);

            Assert.IsTrue(found);
            Assert.AreEqual(3u, first.Version);
            CollectionAssert.AreEqual(SectionFiles.FIRST_DATA, first.Data);
        }

        [Test]
        public void ThenASectionOfAnotherSizeReadsBackBesideIt()
        {
            bool found = m_ReadBack.TryGetValue(Fnv1a64.Hash(SectionFiles.OTHER), out SectionBlob other);

            Assert.IsTrue(found);
            Assert.AreEqual(7u, other.Version);
            CollectionAssert.AreEqual(SectionFiles.OTHER_DATA, other.Data);
        }

        [Test]
        public void ThenASectionNeverWrittenIsMissing()
        {
            Assert.IsFalse(m_ReadBack.ContainsKey(Fnv1a64.Hash("Never.Written")));
        }
    }
}
