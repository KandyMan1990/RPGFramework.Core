using NUnit.Framework;
using RPGFramework.Core.PlayerLoop;

namespace RPGFramework.Core.Tests.PlayerLoop
{
    internal sealed class WhenAModuleRegistersForLateUpdates
    {
        private CountingLateUpdatable m_Module;

        [SetUp]
        public void SetUp()
        {
            UpdateManager.ClearListeners();

            m_Module = new CountingLateUpdatable();

            UpdateManager.RegisterLateUpdatable(m_Module);
        }

        [TearDown]
        public void TearDown()
        {
            UpdateManager.ClearListeners();
        }

        [Test]
        public void ThenItIsTickedOnTheNextLateUpdate()
        {
            UpdateManager.LateUpdateListeners();

            Assert.AreEqual(1, m_Module.Ticks);
        }

        [Test]
        public void ThenTheFrameUpdateDoesNotTickIt()
        {
            UpdateManager.UpdateListeners();

            Assert.AreEqual(0, m_Module.Ticks);
        }

        [Test]
        public void ThenUnregisteringStopsIt()
        {
            UpdateManager.UnregisterLateUpdatable(m_Module);
            UpdateManager.LateUpdateListeners();

            Assert.AreEqual(0, m_Module.Ticks);
        }
    }
}
