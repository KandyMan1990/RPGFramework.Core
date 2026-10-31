using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal sealed class WhenARevealIsOvertaken
    {
        private FakeModuleHost m_Host;
        private ModuleStack    m_Stack;

        [SetUp]
        public void SetUp()
        {
            m_Host  = new FakeModuleHost();
            m_Stack = Stacks.WithTwo(m_Host);

            m_Host.OvertakeReveals();

            Stacks.Apply(m_Stack, ModuleChange.Close());
        }

        [Test]
        public void ThenTheModuleIsNotToldItIsRevealed()
        {
            Assert.AreEqual("cover, 1 exit, unload 1 scene, current 0, 0 resume, reveal overtaken", Stacks.Steps(m_Host));
        }
    }
}
