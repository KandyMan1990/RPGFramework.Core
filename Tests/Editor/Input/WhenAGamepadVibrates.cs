using NUnit.Framework;
using RPGFramework.Core.Input;
using RPGFramework.Core.PlayerLoop;
using RPGFramework.Core.Tests.Store;

namespace RPGFramework.Core.Tests.Input
{
    /// <summary>The motors at 0.5 and 1 for two seconds.</summary>
    internal sealed class WhenAGamepadVibrates
    {
        private FakeGamepadMotors m_Motors;
        private FakeFrameClock    m_Clock;
        private IVibrationService m_Vibration;

        [SetUp]
        public void SetUp()
        {
            UpdateManager.ClearListeners();

            m_Motors    = new FakeGamepadMotors();
            m_Clock     = new FakeFrameClock();
            m_Vibration = new VibrationService(m_Motors, m_Clock);

            m_Vibration.Vibrate(0.5f, 1f, 2f);
        }

        [TearDown]
        public void TearDown()
        {
            m_Vibration.Stop();

            UpdateManager.ClearListeners();
        }

        [Test]
        public void ThenTheMotorsRunAtTheStrengthsGiven()
        {
            Assert.AreEqual(0.5f, m_Motors.Low);
            Assert.AreEqual(1f, m_Motors.High);
        }

        [Test]
        public void ThenTheyKeepRunningUntilTheTimeIsUp()
        {
            Pass(1f);

            Assert.AreEqual(0, m_Motors.Stopped);
        }

        [Test]
        public void ThenTheyStopWhenTheTimeIsUp()
        {
            Pass(2f);

            Assert.AreEqual(1, m_Motors.Stopped);
        }

        [Test]
        public void ThenANewVibrationReplacesTheOne()
        {
            Pass(1f);

            m_Vibration.Vibrate(0.25f, 0.25f, 3f);

            Pass(2f);

            Assert.AreEqual(0.25f, m_Motors.Low);
            Assert.AreEqual(0, m_Motors.Stopped);
        }

        [Test]
        public void ThenNoTimeStopsThem()
        {
            m_Vibration.Vibrate(1f, 1f, 0f);

            Assert.AreEqual(1, m_Motors.Stopped);
        }

        [Test]
        public void ThenStoppingNeedsNoFurtherTicks()
        {
            m_Vibration.Stop();

            Pass(5f);

            Assert.AreEqual(1, m_Motors.Stopped);
        }

        private void Pass(float seconds)
        {
            m_Clock.Seconds = seconds;

            UpdateManager.UpdateListeners();
        }
    }
}
