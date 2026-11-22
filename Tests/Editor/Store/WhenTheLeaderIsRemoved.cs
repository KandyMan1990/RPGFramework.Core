using NUnit.Framework;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>The character in slot 0 taken out of the party.</summary>
    internal sealed class WhenTheLeaderIsRemoved
    {
        private IPartyStore m_Store;

        [SetUp]
        public void SetUp()
        {
            m_Store = Parties.Make(true);
            m_Store.RemoveFromParty(Parties.FIRST);
        }

        [Test]
        public void ThenTheirSlotIsEmpty()
        {
            Assert.AreEqual(-1, m_Store.GetRecordInSlot(0));
            Assert.AreEqual(-1, m_Store.FindSlot(Parties.FIRST));
        }

        [Test]
        public void ThenTheOthersKeepTheirSlots()
        {
            Assert.AreEqual(1, m_Store.FindSlot(Parties.SECOND));
        }

        [Test]
        public void ThenTheyHaveStillJoined()
        {
            Assert.IsTrue(m_Store.HasJoined(0));
        }
    }
}
