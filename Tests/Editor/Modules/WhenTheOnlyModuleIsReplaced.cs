using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal sealed class WhenTheOnlyModuleIsReplaced
    {
        private FakeModuleHost m_Host;
        private ModuleStack    m_Stack;

        [SetUp]
        public void SetUp()
        {
            m_Host  = new FakeModuleHost();
            m_Stack = Stacks.WithOne(m_Host);
        }

        [Test]
        public void ThenTheNextLoadsAlone()
        {
            Stacks.Apply(m_Stack, ModuleChange.Replace(2));

            Assert.AreEqual("cover, 0 exit, load 2 alone, unload 0 container, 2 enter 0, reveal, 2 revealed", Stacks.Steps(m_Host));
        }

        [Test]
        public void ThenAReturnEntersWithItsEntry()
        {
            Stacks.Apply(m_Stack, ModuleChange.Replace(2, ModuleEntries.RETURN));

            Assert.AreEqual("cover, 0 exit, load 2 alone, unload 0 container, 2 enter 1, reveal, 2 revealed", Stacks.Steps(m_Host));
        }
    }
}
