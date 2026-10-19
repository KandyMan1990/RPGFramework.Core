using NUnit.Framework;
using RPGFramework.Core.PlayerLoop;

namespace RPGFramework.Core.Tests.PlayerLoop
{
    internal sealed class WhenAModuleRegistersForPhysicsSteps
    {
        [SetUp]
        public void SetUp()
        {
            UpdateManager.ClearListeners();
        }

        [TearDown]
        public void TearDown()
        {
            UpdateManager.ClearListeners();
        }

        [Test]
        public void ThenItIsTickedOnTheNextStep()
        {
            CountingFixedUpdatable module = new CountingFixedUpdatable();

            UpdateManager.RegisterFixedUpdatable(module);
            UpdateManager.FixedUpdateListeners();

            Assert.AreEqual(1, module.Ticks);
        }
    }
}
