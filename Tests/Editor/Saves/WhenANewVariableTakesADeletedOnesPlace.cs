using System;
using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenANewVariableTakesADeletedOnesPlace
    {
        private byte[] m_Image;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap before = new TestVariableMap().Add("a", MemoryBank.Persistent, VariableWidth.Int, 0, 0)
                                                          .Add("b", MemoryBank.Persistent, VariableWidth.Int, 4, 0);
            TestVariableMap after = new TestVariableMap().Add("a", MemoryBank.Persistent, VariableWidth.Int, 0, 0, id: 1)
                                                         .Add("c", MemoryBank.Persistent, VariableWidth.Int, 4, 7, id: 3);

            m_Image = PersistentMigration.Migrate(new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 }, SaveLayouts.Of(before), after, 8);
        }

        [Test]
        public void ThenItStartsAtItsDefault()
        {
            Assert.AreEqual(7, BitConverter.ToInt32(m_Image, 4));
        }

        [Test]
        public void ThenTheVariableBeforeItKeepsItsValue()
        {
            Assert.AreEqual(1, BitConverter.ToInt32(m_Image, 0));
        }
    }
}
