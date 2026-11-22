using NUnit.Framework;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>A character no record names added to the party, the roster's last record free.</summary>
    internal sealed class WhenACharacterJoinsThePartyWithNoRecord
    {
        private IPartyStore m_Store;

        [SetUp]
        public void SetUp()
        {
            m_Store = Parties.Make(true);
            m_Store.AddToParty(Parties.NEW);
        }

        [Test]
        public void ThenTheyWearTheFreeRecord()
        {
            Assert.AreEqual(3, m_Store.FindRecord(Parties.NEW));
        }

        [Test]
        public void ThenTheyHaveJoined()
        {
            Assert.IsTrue(m_Store.HasJoined(3));
        }

        [Test]
        public void ThenTheyTakeTheEmptySlot()
        {
            Assert.AreEqual(2, m_Store.FindSlot(Parties.NEW));
        }
    }
}
