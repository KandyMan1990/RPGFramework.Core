using NUnit.Framework;
using RPGFramework.Core.PlayerLoop;

namespace RPGFramework.Core.Tests.PlayerLoop
{
    internal sealed class WhenAModuleRegistersAndUnregistersInOneFrame
    {
        private CountingUpdatable m_Module;

        [SetUp]
        public void SetUp()
        {
            UpdateManager.ClearListeners();

            m_Module = new CountingUpdatable();

            UpdateManager.RegisterUpdatable(m_Module);
            UpdateManager.UnregisterUpdatable(m_Module);
            UpdateManager.UpdateListeners();
        }

        [TearDown]
        public void TearDown()
        {
            UpdateManager.ClearListeners();
        }

        [Test]
        public void ThenItIsNotTicked()
        {
            Assert.AreEqual(0, m_Module.Ticks);
        }
    }
}
