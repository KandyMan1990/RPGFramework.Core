using System;
using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASectionFileIsWritten
    {
        private byte[] m_File;

        [SetUp]
        public void SetUp()
        {
            m_File = SectionFiles.TwoSections();
        }

        [Test]
        public void ThenItStartsWithItsMagicNumber()
        {
            Assert.AreEqual(0x53475052u, BitConverter.ToUInt32(m_File, 0));
        }

        [Test]
        public void ThenItReadsAsIntact()
        {
            SectionFileStatus status = SectionFiles.Read(m_File, out Dictionary<ulong, SectionBlob> sections);

            Assert.AreEqual(SectionFileStatus.Intact, status);
            Assert.AreEqual(2, sections.Count);
        }
    }
}
