using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal sealed class WhenTheTopOfTwoIsReplaced
    {
        [Test]
        public void ThenTheOneUnderIsLeftAlone()
        {
            FakeModuleHost host  = new FakeModuleHost();
            ModuleStack    stack = Stacks.WithTwo(host);

            Stacks.Apply(stack, ModuleChange.Replace(3));

            Assert.AreEqual("cover, 1 exit, unload 1 scene, load 3 beside, 3 enter 0, reveal, 3 revealed", Stacks.Steps(host));
        }
    }
}
