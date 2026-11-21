using System;
using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    /// <summary>
    /// An array and a record of records the framework requires at a count the game chooses, beside a record whose count
    /// is the framework's.
    /// </summary>
    internal sealed class WhenARequiredVariablesCountIsTheGames
    {
        private RequiredVariable[] m_Required;

        [SetUp]
        public void SetUp()
        {
            m_Required = new[]
                         {
                             new RequiredVariable("Slots", MemoryBank.Persistent, VariableWidth.Byte, 3, string.Empty, 255),
                             new RequiredVariable("Members", MemoryBank.Persistent, new[] { Field("Who", VariableWidth.ULong), Field("Here", VariableWidth.Bool) }, 8, true, string.Empty),
                             new RequiredVariable("Fixed", MemoryBank.Persistent, new[] { Field("Value", VariableWidth.Byte) }, 2, false, string.Empty)
                         };
        }

        [Test]
        public void ThenAnyCountIsAccepted()
        {
            TestVariableMap map = Map(VariableWidth.Byte, 2, Field("Who", VariableWidth.ULong), Field("Here", VariableWidth.Bool));

            Assert.IsEmpty(Problems(map));
        }

        [Test]
        public void ThenTheGamesOwnFieldsAreAccepted()
        {
            TestVariableMap map = Map(VariableWidth.Byte, 2, Field("Level", VariableWidth.Byte), Field("Who", VariableWidth.ULong), Field("Here", VariableWidth.Bool));

            Assert.IsEmpty(Problems(map));
        }

        [Test]
        public void ThenAFieldTheFrameworkReadsIsStillRequired()
        {
            TestVariableMap map = Map(VariableWidth.Byte, 2, Field("Who", VariableWidth.ULong));

            Assert.AreEqual(1, Problems(map).Length);
        }

        [Test]
        public void ThenTheArraysWidthIsStillRequired()
        {
            TestVariableMap map = Map(VariableWidth.Short, 2, Field("Who", VariableWidth.ULong), Field("Here", VariableWidth.Bool));

            Assert.AreEqual(1, Problems(map).Length);
        }

        [Test]
        public void ThenAFixedCountIsStillHeld()
        {
            TestVariableMap map = Map(VariableWidth.Byte, 3, Field("Who", VariableWidth.ULong), Field("Here", VariableWidth.Bool));

            Assert.AreEqual(1, Problems(map).Length);
        }

        private static TestVariableMap Map(VariableWidth slotWidth, int fixedCount, params VariableRecordField[] members)
        {
            return new TestVariableMap().Add("Slots", MemoryBank.Persistent, slotWidth, 0, 255, 5)
                                        .AddRecord("Members", MemoryBank.Persistent, 16, 12, members)
                                        .AddRecord("Fixed", MemoryBank.Persistent, 256, fixedCount, new[] { Field("Value", VariableWidth.Byte) });
        }

        private string[] Problems(IVariableMap map)
        {
            return VariableMapValidation.Validate(map.Variables, map.LastVariableId, m_Required, Array.Empty<IStartModule>());
        }

        private static VariableRecordField Field(string name, VariableWidth width)
        {
            return new VariableRecordField(name, width, 1, 0);
        }
    }
}
