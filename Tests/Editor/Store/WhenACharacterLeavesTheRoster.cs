using NUnit.Framework;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>A character in the party's second slot leaving.</summary>
    internal sealed class WhenACharacterLeavesTheRoster
    {
        private IPartyStore m_Store;

        [SetUp]
        public void SetUp()
        {
            m_Store = Parties.Make(true);
            m_Store.SetJoined(Parties.SECOND, false);
        }

        [Test]
        public void ThenTheyHaveNotJoined()
        {
            Assert.IsFalse(m_Store.HasJoined(1));
        }

        [Test]
        public void ThenTheyLeaveTheParty()
        {
            Assert.AreEqual(-1, m_Store.FindSlot(Parties.SECOND));
            Assert.AreEqual(-1, m_Store.GetRecordInSlot(1));
        }

        [Test]
        public void ThenTheirRecordIsKept()
        {
            Assert.AreEqual(1, m_Store.FindRecord(Parties.SECOND));
        }
    }
}
