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
            TestVariableMap map = new TestVariableMap().Add(CoreVariables.PLAY_TIME, MemoryBank.Persistent, VariableWidth.UInt, 0, 0);

            m_Clock   = new FakeFrameClock();
            m_Counter = new PlayTimeCounter(TestMemory.Make(32), map, m_Clock);
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
