using NUnit.Framework;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>The party's empty slot filled, then a fourth character added.</summary>
    internal sealed class WhenACharacterIsAddedToAFullParty
    {
        private IPartyStore m_Store;

        [SetUp]
        public void SetUp()
        {
            m_Store = Parties.Make(false);
            m_Store.AddToParty(Parties.THIRD);
            m_Store.AddToParty(Parties.FOURTH);
        }

        [Test]
        public void ThenTheyStillJoin()
        {
            Assert.IsTrue(m_Store.HasJoined(3));
        }

        [Test]
        public void ThenTheyHaveNoSlot()
        {
            Assert.AreEqual(-1, m_Store.FindSlot(Parties.FOURTH));
            Assert.AreEqual(2,  m_Store.FindSlot(Parties.THIRD));
        }
    }
}
