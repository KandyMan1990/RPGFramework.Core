using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The party as the variables hold it: a record for every playable character in <see cref="CoreVariables.ROSTER" />,
    /// and who is in the party, in order, in <see cref="CoreVariables.PARTY" />, slot 0 the leader. A record's other
    /// fields are the game's, read by name from the record this gives.
    /// </summary>
    public interface IPartyStore
    {
        /// <summary>How many slots the party has: the game's party size.</summary>
        int PartySize { get; }

        /// <summary>How many records the roster has.</summary>
        int RosterSize { get; }

        /// <summary>The roster record in a party slot; -1 for an empty slot.</summary>
        int GetRecordInSlot(int slot);

        /// <summary>The character a roster record holds, as its name's hash; zero for a record no one wears.</summary>
        ulong GetCharacter(int record);

        /// <summary>Whether a roster record's character has joined, and so may be put in the party.</summary>
        bool HasJoined(int record);

        /// <summary>The roster record a character holds; -1 for a character no record names.</summary>
        int FindRecord(ulong character);

        /// <summary>The party slot a character is in; -1 for a character not in the party.</summary>
        int FindSlot(ulong character);

        /// <summary>
        /// Puts a character in the first empty slot, joining them too; nothing more for one in the party already. With
        /// no slot empty, they still join.
        /// </summary>
        void AddToParty(ulong character);

        /// <summary>Takes a character out of the party, their slot left empty.</summary>
        void RemoveFromParty(ulong character);

        /// <summary>Fills the party's slots in order with <paramref name="characters" />, each joining, and empties the rest.</summary>
        void SetParty(ulong[] characters);

        /// <summary>
        /// A character joins, or leaves and so leaves the party too, their record kept either way. One no record names
        /// takes the first record no one wears.
        /// </summary>
        void SetJoined(ulong character, bool joined);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.ROSTER" /> and <see cref="CoreVariables.PARTY" />, so a save holds the party and a
    /// new game starts from the map's defaults.
    /// </summary>
    internal sealed class PartyStore : IPartyStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly int            m_PartyOffset;
        private readonly int            m_PartySize;
        private readonly int            m_RosterOffset;
        private readonly int            m_RosterSize;
        private readonly int            m_RecordSize;
        private readonly int            m_CharacterOffset;
        private readonly int            m_JoinedOffset;

        public PartyStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.PARTY,  out VariableDefinition party);
            variableMap.TryGetVariable(CoreVariables.ROSTER, out VariableDefinition roster);

            m_MemoryService   = memoryService;
            m_PartyOffset     = party.Offset;
            m_PartySize       = party.Count;
            m_RosterOffset    = roster.Offset;
            m_RosterSize      = roster.Count;
            m_RecordSize      = roster.ElementSize;
            m_CharacterOffset = roster.GetFieldOffset(CoreVariables.ROSTER_CHARACTER);
            m_JoinedOffset    = roster.GetFieldOffset(CoreVariables.ROSTER_JOINED);
        }

        int IPartyStore.PartySize => m_PartySize;

        int IPartyStore.RosterSize => m_RosterSize;

        int IPartyStore.GetRecordInSlot(int slot) => RecordInSlot(slot);

        ulong IPartyStore.GetCharacter(int record) => CharacterOf(record);

        bool IPartyStore.HasJoined(int record) => m_MemoryService.ReadBool(MemoryBank.Persistent, (ushort)(RecordAt(record) + m_JoinedOffset));

        int IPartyStore.FindRecord(ulong character) => FindRecord(character);

        int IPartyStore.FindSlot(ulong character)
        {
            int record = FindRecord(character);

            return record < 0 ? -1 : SlotOf(record);
        }

        void IPartyStore.AddToParty(ulong character)
        {
            int record = Join(character);

            if (record < 0 || SlotOf(record) >= 0)
            {
                return;
            }

            int empty = SlotOf(-1);

            if (empty >= 0)
            {
                WriteSlot(empty, record);
            }
        }

        void IPartyStore.RemoveFromParty(ulong character)
        {
            int record = FindRecord(character);

            LeaveParty(record);
        }

        void IPartyStore.SetParty(ulong[] characters)
        {
            for (int slot = 0; slot < m_PartySize; slot++)
            {
                WriteSlot(slot, slot < characters.Length ? Join(characters[slot]) : -1);
            }
        }

        void IPartyStore.SetJoined(ulong character, bool joined)
        {
            if (joined)
            {
                Join(character);
                return;
            }

            int record = FindRecord(character);

            if (record < 0)
            {
                return;
            }

            m_MemoryService.WriteBool(MemoryBank.Persistent, (ushort)(RecordAt(record) + m_JoinedOffset), false);

            LeaveParty(record);
        }

        // The record a character joins with, taking the first no one wears when none names them; -1 when none is free.
        private int Join(ulong character)
        {
            int record = FindRecord(character);

            for (int free = 0; free < m_RosterSize && record < 0; free++)
            {
                if (CharacterOf(free) == 0)
                {
                    record = free;

                    m_MemoryService.WriteUlong(MemoryBank.Persistent, (ushort)(RecordAt(record) + m_CharacterOffset), character);
                }
            }

            if (record < 0)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"{CoreVariables.ROSTER} has no record for character {character:X16} to join with, as every record is worn: give the roster more records");
#endif
                return -1;
            }

            m_MemoryService.WriteBool(MemoryBank.Persistent, (ushort)(RecordAt(record) + m_JoinedOffset), true);

            return record;
        }

        private void LeaveParty(int record)
        {
            for (int slot = 0; slot < m_PartySize && record >= 0; slot++)
            {
                if (RecordInSlot(slot) == record)
                {
                    WriteSlot(slot, -1);
                }
            }
        }

        // The first slot holding the record, -1 matching an empty slot.
        private int SlotOf(int record)
        {
            for (int slot = 0; slot < m_PartySize; slot++)
            {
                if (RecordInSlot(slot) == record)
                {
                    return slot;
                }
            }

            return -1;
        }

        private void WriteSlot(int slot, int record)
        {
            m_MemoryService.WriteByte(MemoryBank.Persistent, (ushort)(m_PartyOffset + slot), record < 0 ? CoreVariables.EMPTY_PARTY_SLOT : (byte)record);
        }

        private int RecordInSlot(int slot)
        {
            byte record = m_MemoryService.ReadByte(MemoryBank.Persistent, (ushort)(m_PartyOffset + slot));

            return record == CoreVariables.EMPTY_PARTY_SLOT ? -1 : record;
        }

        private ulong CharacterOf(int record) => m_MemoryService.ReadUlong(MemoryBank.Persistent, (ushort)(RecordAt(record) + m_CharacterOffset));

        private int RecordAt(int record) => m_RosterOffset + record * m_RecordSize;

        // Zero names no character, so a record no one wears is never found.
        private int FindRecord(ulong character)
        {
            for (int record = 0; record < m_RosterSize && character != 0; record++)
            {
                if (CharacterOf(record) == character)
                {
                    return record;
                }
            }

            return -1;
        }
    }
}
