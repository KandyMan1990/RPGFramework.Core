using NUnit.Framework;
using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenADefaultIsStored
    {
        [TestCase(-5L, VariableWidth.SByte)]
        [TestCase(200L, VariableWidth.Byte)]
        [TestCase(-2L, VariableWidth.Short)]
        [TestCase(-2147483648L, VariableWidth.Int)]
        [TestCase(4294967295L, VariableWidth.UInt)]
        public void ThenAnIntegerKeepsItsValue(long value, VariableWidth width)
        {
            ulong stored = VariableDefaults.FromInteger(value, width);

            Assert.AreEqual(value, VariableDefaults.ToInteger(stored, width));
        }

        [Test]
        public void ThenAFloatKeepsItsValue()
        {
            Assert.AreEqual(-1.25f, VariableDefaults.ToFloat(VariableDefaults.FromFloat(-1.25f)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ThenABoolKeepsItsValue(bool value)
        {
            Assert.AreEqual(value, VariableDefaults.ToBool(VariableDefaults.FromBool(value)));
        }
    }
}
