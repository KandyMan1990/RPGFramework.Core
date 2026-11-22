using NUnit.Framework;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>A party of three set to two characters, the first not joined before.</summary>
    internal sealed class WhenThePartyIsSet
    {
        private IPartyStore m_Store;

        [SetUp]
        public void SetUp()
        {
            m_Store = Parties.Make(true);
            m_Store.SetParty(new[] { Parties.THIRD, Parties.FIRST });
        }

        [Test]
        public void ThenTheSlotsHoldThemInOrder()
        {
            Assert.AreEqual(2, m_Store.GetRecordInSlot(0));
            Assert.AreEqual(0, m_Store.GetRecordInSlot(1));
        }

        [Test]
        public void ThenTheRestAreEmpty()
        {
            Assert.AreEqual(-1, m_Store.GetRecordInSlot(2));
            Assert.AreEqual(-1, m_Store.FindSlot(Parties.SECOND));
        }

        [Test]
        public void ThenEachHasJoined()
        {
            Assert.IsTrue(m_Store.HasJoined(2));
        }
    }
}
