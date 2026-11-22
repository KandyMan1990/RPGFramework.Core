using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>
    /// A roster of four records — the first two joined and in the party's first two slots, the third not joined, the
    /// fourth free or worn by a character not joined — and a party of three, its last slot empty.
    /// </summary>
    internal static class Parties
    {
        internal const ulong FIRST  = 0x1111;
        internal const ulong SECOND = 0x2222;
        internal const ulong THIRD  = 0x3333;
        internal const ulong FOURTH = 0x4444;
        internal const ulong NEW    = 0x5555;

        private const ushort PARTY  = 0;
        private const ushort ROSTER = 8;
        private const int    RECORD = 9;

        internal static IPartyStore Make(bool lastRecordFree)
        {
            VariableRecordField[] fields =
            {
                new VariableRecordField(CoreVariables.ROSTER_CHARACTER, VariableWidth.ULong, 1, 0),
                new VariableRecordField(CoreVariables.ROSTER_JOINED, VariableWidth.Bool, 1, 0)
            };

            TestVariableMap map = new TestVariableMap().Add(CoreVariables.PARTY, MemoryBank.Persistent, VariableWidth.Byte, PARTY, 255, 3)
                                                       .AddRecord(CoreVariables.ROSTER, MemoryBank.Persistent, ROSTER, 4, fields);

            IMemoryService memory = TestMemory.Make(64);

            Record(memory, 0, FIRST,  true);
            Record(memory, 1, SECOND, true);
            Record(memory, 2, THIRD,  false);
            Record(memory, 3, lastRecordFree ? 0UL : FOURTH, false);

            memory.WriteByte(MemoryBank.Persistent, PARTY,     0);
            memory.WriteByte(MemoryBank.Persistent, PARTY + 1, 1);
            memory.WriteByte(MemoryBank.Persistent, PARTY + 2, CoreVariables.EMPTY_PARTY_SLOT);

            return new PartyStore(memory, map);
        }

        private static void Record(IMemoryService memory, int record, ulong character, bool joined)
        {
            ushort start = (ushort)(ROSTER + record * RECORD);

            memory.WriteUlong(MemoryBank.Persistent, start, character);
            memory.WriteBool(MemoryBank.Persistent, (ushort)(start + 8), joined);
        }
    }
}
