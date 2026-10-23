using NUnit.Framework;
using RPGFramework.Core.PlayerLoop;

namespace RPGFramework.Core.Tests.PlayerLoop
{
    internal sealed class WhenAModuleRegistersTwice
    {
        private CountingUpdatable m_Module;

        [SetUp]
        public void SetUp()
        {
            UpdateManager.ClearListeners();

            m_Module = new CountingUpdatable();

            UpdateManager.RegisterUpdatable(m_Module);
            UpdateManager.RegisterUpdatable(m_Module);
            UpdateManager.UpdateListeners();
        }

        [TearDown]
        public void TearDown()
        {
            UpdateManager.ClearListeners();
        }

        [Test]
        public void ThenItIsTickedOnce()
        {
            Assert.AreEqual(1, m_Module.Ticks);
        }
    }
}
