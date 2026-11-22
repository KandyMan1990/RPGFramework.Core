using System.Text.RegularExpressions;
using NUnit.Framework;
using RPGFramework.Core.Store;
using UnityEngine;
using UnityEngine.TestTools;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>A character no record names added to the party, every record worn.</summary>
    internal sealed class WhenEveryRecordIsWorn
    {
        private IPartyStore m_Store;

        [SetUp]
        public void SetUp()
        {
            m_Store = Parties.Make(false);
        }

        [Test]
        public void ThenTheyCannotJoin()
        {
            LogAssert.Expect(LogType.Warning, new Regex("no record for character"));

            m_Store.AddToParty(Parties.NEW);

            Assert.AreEqual(-1, m_Store.FindRecord(Parties.NEW));
            Assert.AreEqual(-1, m_Store.GetRecordInSlot(2));
        }
    }
}
