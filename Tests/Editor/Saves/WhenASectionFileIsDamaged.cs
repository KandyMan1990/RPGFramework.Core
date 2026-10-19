using System;
using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASectionFileIsDamaged
    {
        private byte[] m_File;

        [SetUp]
        public void SetUp()
        {
            m_File = SectionFiles.TwoSections();
        }

        [Test]
        public void ThenAChangedByteIsDamageAndNothingIsRead()
        {
            m_File[m_File.Length - 1] ^= 0xFF;

            SectionFileStatus status = SectionFiles.Read(m_File, out Dictionary<ulong, SectionBlob> sections);

            Assert.AreEqual(SectionFileStatus.Damaged, status);
            Assert.AreEqual(0, sections.Count);
        }

        [Test]
        public void ThenAFileCutShortIsDamage()
        {
            byte[] cut = new byte[m_File.Length - 5];

            Array.Copy(m_File, cut, cut.Length);

            Assert.AreEqual(SectionFileStatus.Damaged, SectionFiles.Read(cut, out Dictionary<ulong, SectionBlob> _));
        }
    }
}
