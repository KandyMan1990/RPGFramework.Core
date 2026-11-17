using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>The player's message speed setting turned into characters a second, over a range of 22.5 to 52.5.</summary>
    internal sealed class WhenTheMessageSpeedIsSet
    {
        private IMessageSpeedStore m_Store;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = new TestVariableMap().Add(CoreVariables.MESSAGE_SPEED_SLOWEST, MemoryBank.Session, VariableWidth.Float, 0, VariableDefaults.FromFloat(22.5f))
                                                       .Add(CoreVariables.MESSAGE_SPEED_FASTEST, MemoryBank.Session, VariableWidth.Float, 4, VariableDefaults.FromFloat(52.5f));

            MemoryService memory = TestMemory.Make(16);

            VariableDefaults.Write(memory, map, MemoryBank.Session);

            m_Store = new MessageSpeedStore(memory, map);
        }

        [Test]
        public void ThenTheLeftIsTheSlowest()
        {
            Assert.AreEqual(22.5f, m_Store.GetCharactersPerSecond(0f));
        }

        [Test]
        public void ThenTheRightIsTheFastest()
        {
            Assert.AreEqual(52.5f, m_Store.GetCharactersPerSecond(1f));
        }

        [Test]
        public void ThenTheMiddleIsHalfway()
        {
            Assert.AreEqual(37.5f, m_Store.GetCharactersPerSecond(0.5f));
        }
    }
}
