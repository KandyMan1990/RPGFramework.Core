using NUnit.Framework;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>A countdown of three seconds, hidden, as the game plays on.</summary>
    internal sealed class WhenTheCountdownRuns
    {
        private FakeFrameClock  m_Clock;
        private PlayTimeCounter m_Counter;
        private ICountdownStore m_Countdown;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map    = Clocks.Map();
            MemoryService   memory = TestMemory.Make(32);

            m_Clock     = new FakeFrameClock();
            m_Countdown = new CountdownStore(memory, map);
            m_Counter   = new PlayTimeCounter(memory, map, m_Clock, m_Countdown);

            m_Countdown.SetSeconds(3);
        }

        [Test]
        public void ThenItCountsDownEachWholeSecond()
        {
            Frames(0.6f, 0.6f);

            Assert.AreEqual(2u, m_Countdown.Seconds);
        }

        [Test]
        public void ThenItCountsWhileHidden()
        {
            Frames(1f);

            Assert.IsFalse(m_Countdown.IsShown);
            Assert.AreEqual(2u, m_Countdown.Seconds);
        }

        [Test]
        public void ThenItStopsAtZero()
        {
            Frames(2f, 5f);

            Assert.AreEqual(0u, m_Countdown.Seconds);
        }

        [Test]
        public void ThenThePlayTimeCountsOnPastIt()
        {
            Frames(2f, 5f);

            Assert.AreEqual(7u, ((IPlayTimeStore)m_Counter).PlayTime);
        }

        private void Frames(params float[] seconds)
        {
            for (int i = 0; i < seconds.Length; i++)
            {
                m_Clock.Seconds = seconds[i];

                ((IUpdatable)m_Counter).Update();
            }
        }
    }
}
