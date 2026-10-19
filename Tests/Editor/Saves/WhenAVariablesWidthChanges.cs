using System;
using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenAVariablesWidthChanges
    {
        [Test]
        public void ThenNarrowingClampsToTheNewRange()
        {
            Assert.AreEqual(sbyte.MaxValue, (sbyte)Converted(VariableWidth.Byte, new byte[] { 200 }, VariableWidth.SByte)[0]);
            Assert.AreEqual(ushort.MaxValue, BitConverter.ToUInt16(Converted(VariableWidth.UInt, BitConverter.GetBytes(70000u), VariableWidth.UShort), 0));
            Assert.AreEqual(long.MaxValue, BitConverter.ToInt64(Converted(VariableWidth.ULong, BitConverter.GetBytes(ulong.MaxValue), VariableWidth.Long), 0));
        }

        [Test]
        public void ThenANegativeValueMadeUnsignedIsZero()
        {
            Assert.AreEqual(0, Converted(VariableWidth.SByte, new[] { unchecked((byte)-5) }, VariableWidth.Byte)[0]);
            Assert.AreEqual(0UL, BitConverter.ToUInt64(Converted(VariableWidth.Long, BitConverter.GetBytes(-1L), VariableWidth.ULong), 0));
        }

        [Test]
        public void ThenWideningKeepsTheSign()
        {
            Assert.AreEqual(-2, BitConverter.ToInt32(Converted(VariableWidth.Short, BitConverter.GetBytes((short)-2), VariableWidth.Int), 0));
        }

        [TestCase(-3.7f, -3)]
        [TestCase(1e20f, int.MaxValue)]
        [TestCase(float.NaN, 0)]
        public void ThenAFloatMadeAnIntegerTruncatesAndClamps(float value, int expected)
        {
            Assert.AreEqual(expected, BitConverter.ToInt32(Converted(VariableWidth.Float, BitConverter.GetBytes(value), VariableWidth.Int), 0));
        }

        [Test]
        public void ThenAnythingNonZeroIsATrueBool()
        {
            Assert.AreEqual(1, Converted(VariableWidth.Float, BitConverter.GetBytes(0.5f), VariableWidth.Bool)[0]);
            Assert.AreEqual(1, Converted(VariableWidth.Int, BitConverter.GetBytes(5), VariableWidth.Bool)[0]);
        }

        [Test]
        public void ThenNaNIsAFalseBool()
        {
            Assert.AreEqual(0, Converted(VariableWidth.Float, BitConverter.GetBytes(float.NaN), VariableWidth.Bool)[0]);
        }

        [Test]
        public void ThenAnIntegerMadeAFloatKeepsItsValue()
        {
            Assert.AreEqual(7f, BitConverter.ToSingle(Converted(VariableWidth.Int, BitConverter.GetBytes(7), VariableWidth.Float), 0));
        }

        [Test]
        public void ThenABoolMadeAnIntegerIsZeroOrOne()
        {
            Assert.AreEqual(1, BitConverter.ToInt32(Converted(VariableWidth.Bool, new byte[] { 3 }, VariableWidth.Int), 0));
        }

        private static byte[] Converted(VariableWidth from, byte[] value, VariableWidth to)
        {
            TestVariableMap was = new TestVariableMap().Add("v", MemoryBank.Persistent, from, 0, 0);
            TestVariableMap now = new TestVariableMap().Add("v", MemoryBank.Persistent, to, 0, 0, id: 1);
            byte[]          old = new byte[8];

            value.CopyTo(old, 0);

            byte[] image = PersistentMigration.Migrate(old, SaveLayouts.Of(was), now, 8);

            return image;
        }
    }
}
