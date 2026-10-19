using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal static class Records
    {
        /// <summary>hp at 90, a level at 1 and four stats at 5: seven bytes a record.</summary>
        internal static VariableRecordField[] CharacterFields()
        {
            VariableRecordField[] fields =
            {
                new VariableRecordField("hp", VariableWidth.UShort, 1, 90),
                new VariableRecordField("level", VariableWidth.Byte, 1, 1),
                new VariableRecordField("stats", VariableWidth.Byte, 4, 5)
            };

            return fields;
        }

        /// <summary>Three characters from offset 4, and a byte after them.</summary>
        internal static TestVariableMap Characters()
        {
            TestVariableMap map = new TestVariableMap().AddRecord("Characters", MemoryBank.Persistent, 4, 3, CharacterFields())
                                                       .Add("After", MemoryBank.Persistent, VariableWidth.Byte, 25, 3);

            return map;
        }
    }
}
