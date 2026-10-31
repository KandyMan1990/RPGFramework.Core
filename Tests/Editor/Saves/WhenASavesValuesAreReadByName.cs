using System;
using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    /// <summary>A save previewed and read as scripts name its values: a variable, an element and a record's fields.</summary>
    internal sealed class WhenASavesValuesAreReadByName
    {
        private SavePreview m_Preview;

        [SetUp]
        public void SetUp()
        {
            VariableRecordField[] party =
            {
                SaveLayouts.Field("Level", VariableWidth.Byte,   1, 0, 1),
                SaveLayouts.Field("Stats", VariableWidth.UShort, 2, 0, 2)
            };

            TestVariableMap map = new TestVariableMap().Add("Money", MemoryBank.Persistent, VariableWidth.UInt, 0, 0)
                                                       .Add("Items", MemoryBank.Persistent, VariableWidth.SByte, 4, 0, 3)
                                                       .AddRecord("Party", MemoryBank.Persistent, 8, 2, party)
                                                       .Add("Volume", MemoryBank.Settings, VariableWidth.Float, 0, 0);

            byte[] persistent = new byte[18];

            BitConverter.GetBytes(12345u).CopyTo(persistent, 0);
            persistent[6] = unchecked((byte)-4);
            persistent[13] = 42;
            BitConverter.GetBytes((ushort)900).CopyTo(persistent, 16);

            m_Preview = new SavePreview("save000.sav", DateTime.MinValue, persistent, map, SaveState.Loadable);
        }

        [Test]
        public void ThenAVariableIsRead()
        {
            Assert.AreEqual(12345u, m_Preview.Read<uint>("Money"));
        }

        [Test]
        public void ThenAnElementIsRead()
        {
            Assert.AreEqual(-4, m_Preview.ReadNumber("Items[2]"));
        }

        [Test]
        public void ThenARecordsFieldIsRead()
        {
            Assert.AreEqual(42, m_Preview.ReadNumber("Party[1].Level"));
        }

        [Test]
        public void ThenAnElementOfAFieldIsRead()
        {
            Assert.AreEqual(900, m_Preview.ReadNumber("Party[1].Stats[1]"));
        }

        [TestCase("Nothing",          "holds no variable 'Nothing'")]
        [TestCase("Volume",           "holds no variable 'Volume'")]
        [TestCase("Items[3]",         "holds 3 element(s)")]
        [TestCase("Items[x]",         "a whole number in brackets")]
        [TestCase("Items[",           "a whole number in brackets")]
        [TestCase("Money.Level",      "not a record")]
        [TestCase("Party[0]",         "name one of its fields")]
        [TestCase("Party[0].Gone",    "no field called 'Gone'")]
        [TestCase("Party[0].Stats[2]", "'Stats' holds 2 element(s)")]
        public void ThenANameThatMissesSaysWhy(string path, string why)
        {
            ArgumentException missed = Assert.Throws<ArgumentException>(() => m_Preview.ReadNumber(path));

            StringAssert.Contains(why, missed.Message);
        }
    }
}
