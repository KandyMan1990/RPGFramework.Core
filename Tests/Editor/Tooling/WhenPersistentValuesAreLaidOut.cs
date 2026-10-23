using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Tooling
{
    internal sealed class WhenPersistentValuesAreLaidOut
    {
        private IPersistentValues m_Values;

        [SetUp]
        public void SetUp()
        {
            VariableRecordField[] party =
            {
                SaveLayouts.Field("hp", VariableWidth.UShort, 1, 50, 1),
                SaveLayouts.Field("items", VariableWidth.Byte, 2, 0, 2)
            };

            TestVariableMap map = new TestVariableMap().Add("Gil", MemoryBank.Persistent, VariableWidth.UInt, 0, 100)
                                                       .Add("Steps", MemoryBank.Session, VariableWidth.Byte, 0, 0)
                                                       .Add("Flags", MemoryBank.Persistent, VariableWidth.Bool, 4, 0, 3)
                                                       .AddRecord("Party", MemoryBank.Persistent, 8, 2, party);

            m_Values = new PersistentValues(map, new FakeMemoryArgs(16));
        }

        [Test]
        public void ThenSessionVariablesAreLeftOut()
        {
            Assert.IsFalse(Labels().Contains("Steps"));
        }

        [Test]
        public void ThenAnArrayHasAValuePerElement()
        {
            CollectionAssert.IsSubsetOf(new[] { "Flags[0]", "Flags[1]", "Flags[2]" }, Labels());
        }

        [Test]
        public void ThenEachRecordsFieldsAndTheirElementsHaveAValueEach()
        {
            CollectionAssert.IsSubsetOf(new[] { "Party[0].hp", "Party[0].items[0]", "Party[0].items[1]", "Party[1].hp", "Party[1].items[0]", "Party[1].items[1]" }, Labels());
        }

        [Test]
        public void ThenAValueIsWrittenWhereTheGameReadsIt()
        {
            byte[] bank = new byte[16];

            m_Values.Write(bank, Find("Party[1].items[1]"), 9);

            Assert.AreEqual(9, bank[15]);
        }

        [TestCase("Gil", 100UL)]
        [TestCase("Party[1].hp", 50UL)]
        public void ThenEachValueStartsAtItsDefault(string label, ulong expected)
        {
            Assert.AreEqual(expected, Find(label).Default);
        }

        [TestCase("Gil", true)]
        [TestCase("Flags[0]", false)]
        [TestCase("Party[0].hp", false)]
        public void ThenOnlyAWholeVariableIsOneValue(string label, bool expected)
        {
            Assert.AreEqual(expected, Find(label).IsWholeVariable);
        }

        private List<string> Labels()
        {
            List<string> labels = new List<string>();

            for (int i = 0; i < m_Values.Values.Count; i++)
            {
                labels.Add(m_Values.Values[i].Label);
            }

            return labels;
        }

        private PersistentValue Find(string label)
        {
            for (int i = 0; i < m_Values.Values.Count; i++)
            {
                if (m_Values.Values[i].Label == label)
                {
                    return m_Values.Values[i];
                }
            }

            Assert.Fail($"No value labelled {label}");

            return default;
        }
    }
}
