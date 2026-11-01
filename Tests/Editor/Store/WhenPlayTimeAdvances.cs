using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    internal sealed class WhenPlayTimeAdvances
    {
        private FakeFrameClock  m_Clock;
        private PlayTimeCounter m_Counter;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map    = Clocks.Map();
            MemoryService   memory = TestMemory.Make(32);

            m_Clock   = new FakeFrameClock();
            m_Counter = new PlayTimeCounter(memory, map, m_Clock, new CountdownStore(memory, map));
        }

        [Test]
        public void ThenLessThanASecondCountsNothing()
        {
            Frames(0.6f);

            Assert.AreEqual(0u, PlayTime());
        }

        [Test]
        public void ThenTheFractionCarriesIntoTheNextSecond()
        {
            Frames(0.6f, 0.6f);

            Assert.AreEqual(1u, PlayTime());
        }

        [Test]
        public void ThenOnlyWholeSecondsAreCounted()
        {
            Frames(0.6f, 0.6f, 2.9f);

            Assert.AreEqual(4u, PlayTime());
        }

        private void Frames(params float[] seconds)
        {
            for (int i = 0; i < seconds.Length; i++)
            {
                m_Clock.Seconds = seconds[i];

                ((IUpdatable)m_Counter).Update();
            }
        }

        private uint PlayTime()
        {
            uint playTime = ((IPlayTimeStore)m_Counter).PlayTime;

            return playTime;
        }
    }
}
