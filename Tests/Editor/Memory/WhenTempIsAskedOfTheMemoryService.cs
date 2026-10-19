using System;
using NUnit.Framework;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenTempIsAskedOfTheMemoryService
    {
        [Test]
        public void ThenItIsRefused()
        {
            IMemoryService memory = TestMemory.Make(64);

            Assert.Throws<ArgumentOutOfRangeException>(() => memory.WriteByte(MemoryBank.Temp, 0, 0xAB));
        }
    }
}
