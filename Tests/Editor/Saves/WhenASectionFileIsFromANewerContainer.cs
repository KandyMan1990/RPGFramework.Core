using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASectionFileIsFromANewerContainer
    {
        [Test]
        public void ThenItIsFromANewerVersion()
        {
            byte[] newer = SectionFiles.TwoSections();

            newer[4] = 2;

            Assert.AreEqual(SectionFileStatus.FromNewerVersion, SectionFiles.Read(newer, out Dictionary<ulong, SectionBlob> _));
        }
    }
}
