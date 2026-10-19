using NUnit.Framework;
using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenTempMemoryIsMade
    {
        [Test]
        public void ThenItIsZero()
        {
            TempMemory temp = new TempMemory(16);

            Assert.AreEqual(0UL, temp.ReadUlong(0));
            Assert.AreEqual(0UL, temp.ReadUlong(8));
        }
    }
}
