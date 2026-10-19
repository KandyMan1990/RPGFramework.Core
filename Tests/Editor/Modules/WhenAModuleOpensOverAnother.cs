using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal sealed class WhenAModuleOpensOverAnother
    {
        private FakeModuleHost m_Host;
        private ModuleStack    m_Stack;

        [SetUp]
        public void SetUp()
        {
            m_Host  = new FakeModuleHost();
            m_Stack = Stacks.WithOne(m_Host);

            Stacks.Apply(m_Stack, ModuleChange.Over(1));
        }

        [Test]
        public void ThenTheOneUnderIsSuspendedBeforeItLoads()
        {
            Assert.AreEqual("cover, 0 suspend, load 1 beside, 1 enter 0, reveal, 1 revealed", Stacks.Steps(m_Host));
        }

        [Test]
        public void ThenItIsOnTop()
        {
            Assert.AreEqual(1, m_Stack.TopModuleId);
        }
    }
}
