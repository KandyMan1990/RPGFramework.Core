using System;
using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASectionFileIsFromBeforeTheHeader
    {
        private byte[] m_File;

        // A section count, one entry of id, version, offset and size, then its bytes.
        [SetUp]
        public void SetUp()
        {
            m_File = new byte[4 + 20 + 8];

            BitConverter.GetBytes(1).CopyTo(m_File, 0);
            BitConverter.GetBytes(77UL).CopyTo(m_File, 4);
            BitConverter.GetBytes(3u).CopyTo(m_File, 12);
            BitConverter.GetBytes(24).CopyTo(m_File, 16);
            BitConverter.GetBytes(8).CopyTo(m_File, 20);
            BitConverter.GetBytes(0x0102030405060708L).CopyTo(m_File, 24);
        }

        [Test]
        public void ThenItStillReads()
        {
            SectionFileStatus status = SectionFiles.Read(m_File, out Dictionary<ulong, SectionBlob> sections);

            Assert.AreEqual(SectionFileStatus.Intact, status);
            Assert.IsTrue(sections.ContainsKey(77UL));
        }

        [Test]
        public void ThenASectionPastItsEndIsDamage()
        {
            BitConverter.GetBytes(30).CopyTo(m_File, 16);

            Assert.AreEqual(SectionFileStatus.Damaged, SectionFiles.Read(m_File, out Dictionary<ulong, SectionBlob> _));
        }
    }
}
