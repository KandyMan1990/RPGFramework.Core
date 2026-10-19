using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal sealed class WhenTheFirstModuleOpens
    {
        private FakeModuleHost m_Host;
        private ModuleStack    m_Stack;

        [SetUp]
        public void SetUp()
        {
            m_Host  = new FakeModuleHost();
            m_Stack = new ModuleStack(m_Host);

            Stacks.Apply(m_Stack, ModuleChange.Clear(0));
        }

        [Test]
        public void ThenItLoadsAloneAndEntersUnderACoverAndIsToldOnceRevealed()
        {
            Assert.AreEqual("cover, load 0 alone, 0 enter 0, reveal, 0 revealed", Stacks.Steps(m_Host));
        }

        [Test]
        public void ThenItIsOnTop()
        {
            Assert.AreEqual(0, m_Stack.TopModuleId);
        }
    }
}
