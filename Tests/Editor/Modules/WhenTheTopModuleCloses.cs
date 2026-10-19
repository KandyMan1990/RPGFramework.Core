using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal sealed class WhenTheTopModuleCloses
    {
        private FakeModuleHost m_Host;
        private ModuleStack    m_Stack;

        [SetUp]
        public void SetUp()
        {
            m_Host  = new FakeModuleHost();
            m_Stack = Stacks.WithTwo(m_Host);

            Stacks.Apply(m_Stack, ModuleChange.Close());
        }

        [Test]
        public void ThenItExitsAndUnloadsBeforeTheOneUnderResumes()
        {
            Assert.AreEqual("cover, 1 exit, unload 1 scene, current 0, 0 resume, reveal, 0 revealed", Stacks.Steps(m_Host));
        }

        [Test]
        public void ThenTheOneUnderIsOnTopAgain()
        {
            Assert.AreEqual(0, m_Stack.TopModuleId);
        }
    }
}
