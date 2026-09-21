using System;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// One named variable in a <see cref="VariableMapAsset" />.<br /><br />
    /// <see cref="Offset" /> is assigned by the map, never typed by hand — see
    /// <see cref="VariableMapAsset" /> for why.
    /// </summary>
    [Serializable]
    public sealed class VariableDefinition
    {
        [SerializeField]
        [Tooltip("Name used to reference this variable when authoring. Must be unique within the map")]
        private string m_Name;

        [SerializeField]
        [Tooltip("Which bank this variable lives in. Persistent is saved, Session is not")]
        private MemoryBank m_Bank;

        [SerializeField]
        [Tooltip("Storage width. Determines which IMemoryService accessor reads this variable")]
        private VariableWidth m_Width;

        [SerializeField]
        [Tooltip("Byte offset into the bank. Assigned by the map, do not edit by hand")]
        private int m_Offset;

        [SerializeField]
        [TextArea(1, 3)]
        [Tooltip("What this variable is for. Shown in authoring tools")]
        private string m_Description;

        [SerializeField]
        [Tooltip("What a new game starts with, held as the bytes it occupies in the bank. Edit it through the Variable Map inspector, which shows it as the variable's own type")]
        private ulong m_DefaultValue;

        public string        Name        => m_Name;
        public MemoryBank    Bank        => m_Bank;
        public VariableWidth Width       => m_Width;
        public int           Offset      => m_Offset;
        public string        Description => m_Description;

        /// <summary>
        /// What a new game starts with, as the bytes the variable occupies in its bank, lowest byte first —
        /// see <see cref="VariableDefaults" /> for reading and writing it as a typed value.
        /// </summary>
        public ulong DefaultValue => m_DefaultValue;

        /// <summary>
        /// The first byte after this variable, i.e. <see cref="Offset" /> plus its width in bytes.
        /// </summary>
        public int EndOffset
        {
            get
            {
                int endOffset = m_Offset + m_Width.GetByteCount();

                return endOffset;
            }
        }

        public VariableDefinition(string name, MemoryBank bank, VariableWidth width, int offset, string description, ulong defaultValue)
        {
            m_Name         = name;
            m_Bank         = bank;
            m_Width        = width;
            m_Offset       = offset;
            m_Description  = description;
            m_DefaultValue = defaultValue;
        }

        /// <summary>
        /// Does this variable's byte range overlap <paramref name="other" />'s? Two variables in different
        /// banks never overlap, since each bank is a separate array.
        /// </summary>
        internal bool Overlaps(VariableDefinition other)
        {
            if (m_Bank != other.m_Bank)
            {
                return false;
            }

            bool overlaps = m_Offset < other.EndOffset && other.m_Offset < EndOffset;

            return overlaps;
        }
    }
}