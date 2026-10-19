using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal sealed class WhenTheStackIsCleared
    {
        [Test]
        public void ThenEveryModuleExitsTopFirstBeforeTheNextLoadsAlone()
        {
            FakeModuleHost host  = new FakeModuleHost();
            ModuleStack    stack = Stacks.WithTwo(host);

            Stacks.Apply(stack, ModuleChange.Clear(4));

            Assert.AreEqual("cover, 1 exit, 0 exit, load 4 alone, unload 1 container, unload 0 container, 4 enter 0, reveal, 4 revealed", Stacks.Steps(host));
        }
    }
}
