using NUnit.Framework;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>The leader added to the party again.</summary>
    internal sealed class WhenACharacterInThePartyIsAdded
    {
        private IPartyStore m_Store;

        [SetUp]
        public void SetUp()
        {
            m_Store = Parties.Make(true);
            m_Store.AddToParty(Parties.FIRST);
        }

        [Test]
        public void ThenTheyKeepTheirSlot()
        {
            Assert.AreEqual(0, m_Store.FindSlot(Parties.FIRST));
        }

        [Test]
        public void ThenTheEmptySlotStaysEmpty()
        {
            Assert.AreEqual(-1, m_Store.GetRecordInSlot(2));
        }
    }
}
