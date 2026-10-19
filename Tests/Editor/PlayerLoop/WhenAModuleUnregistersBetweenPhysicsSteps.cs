using NUnit.Framework;
using RPGFramework.Core.PlayerLoop;

namespace RPGFramework.Core.Tests.PlayerLoop
{
    /// <summary>
    /// Frame updates run after that frame's physics steps, so an unregister queued in one has to be applied before the
    /// next step ticks — or a field suspending for a menu moves one more step after capturing what it resumes from.
    /// </summary>
    internal sealed class WhenAModuleUnregistersBetweenPhysicsSteps
    {
        private CountingFixedUpdatable m_Module;

        [SetUp]
        public void SetUp()
        {
            UpdateManager.ClearListeners();

            m_Module = new CountingFixedUpdatable();

            UpdateManager.RegisterFixedUpdatable(m_Module);
            UpdateManager.FixedUpdateListeners();
            UpdateManager.UnregisterFixedUpdatable(m_Module);
            UpdateManager.FixedUpdateListeners();
        }

        [TearDown]
        public void TearDown()
        {
            UpdateManager.ClearListeners();
        }

        [Test]
        public void ThenItGetsNoFurtherStep()
        {
            Assert.AreEqual(1, m_Module.Ticks);
        }
    }
}
