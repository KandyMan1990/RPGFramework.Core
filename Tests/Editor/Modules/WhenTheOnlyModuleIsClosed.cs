using System;
using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal sealed class WhenTheOnlyModuleIsClosed
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
        public void ThenItIsRefused()
        {
            Assert.Throws<InvalidOperationException>(() => Stacks.Apply(m_Stack, ModuleChange.Close()));
        }

        [Test]
        public void ThenTheScreenIsNeverCovered()
        {
            try
            {
                Stacks.Apply(m_Stack, ModuleChange.Close());
            }
            catch (InvalidOperationException)
            {
            }

            CollectionAssert.IsEmpty(m_Host.Log);
        }
    }
}
