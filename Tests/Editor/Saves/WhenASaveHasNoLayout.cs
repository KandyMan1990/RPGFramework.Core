using System;
using NUnit.Framework;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASaveHasNoLayout
    {
        private byte[] m_Image;

        // Written before layouts were recorded: read by the map's own layout, as far as its four bytes reach.
        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = SaveLayouts.Version2();

            m_Image = BankMigration.Migrate(new byte[] { 1, 0, 44, 1 }, BankLayout.Within(map, MemoryBank.Persistent, 4), map, MemoryBank.Persistent, 36);
        }

        [Test]
        public void ThenWhatItReachesIsRead()
        {
            Assert.AreEqual(1, m_Image[0]);
            Assert.AreEqual(300, BitConverter.ToUInt16(m_Image, 2));
        }

        [Test]
        public void ThenWhatItDoesNotReachTakesItsDefault()
        {
            Assert.AreEqual(7, BitConverter.ToUInt16(m_Image, 4));
            Assert.AreEqual(42, m_Image[29]);
        }
    }
}
