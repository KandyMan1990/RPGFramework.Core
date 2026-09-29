using System;
using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// One named variable in a <see cref="VariableMapAsset" />: a single value, or an array of <see cref="Count" />
    /// values of its width laid end to end.<br /><br />
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
        [Tooltip("How many values of its width this variable holds. 1 is a single value; more makes an array, its elements end to end")]
        private int m_Count;

        [SerializeField]
        [Tooltip("Byte offset into the bank. Assigned by the map, do not edit by hand")]
        private int m_Offset;

        [SerializeField]
        [TextArea(1, 3)]
        [Tooltip("What this variable is for. Shown in authoring tools")]
        private string m_Description;

        [SerializeField]
        [Tooltip("What a new game starts with, held as the bytes it occupies in the bank — every element of an array. Edit it through the Variable Map inspector, which shows it as the variable's own type")]
        private ulong m_DefaultValue;

        [SerializeField]
        [Tooltip("Elements of an array that start at something other than the default. Edit them through the Variable Map inspector")]
        private List<VariableElementDefault> m_ElementDefaults = new List<VariableElementDefault>();

        public string        Name        => m_Name;
        public MemoryBank    Bank        => m_Bank;
        public VariableWidth Width       => m_Width;
        public int           Count       => m_Count;
        public int           Offset      => m_Offset;
        public string        Description => m_Description;

        /// <summary>
        /// What a new game starts with, as the bytes the variable occupies in its bank, lowest byte first —
        /// see <see cref="VariableDefaults" /> for reading and writing it as a typed value.
        /// </summary>
        public ulong DefaultValue => m_DefaultValue;

        internal IReadOnlyList<VariableElementDefault> ElementDefaults => m_ElementDefaults;

        /// <summary>
        /// The first byte after this variable, i.e. <see cref="Offset" /> plus its width in bytes for every element.
        /// </summary>
        public int EndOffset
        {
            get
            {
                int endOffset = m_Offset + m_Width.GetByteCount() * m_Count;

                return endOffset;
            }
        }

        public VariableDefinition(string name, MemoryBank bank, VariableWidth width, int count, int offset, string description, ulong defaultValue)
        {
            m_Name         = name;
            m_Bank         = bank;
            m_Width        = width;
            m_Count        = count;
            m_Offset       = offset;
            m_Description  = description;
            m_DefaultValue = defaultValue;
        }

        /// <summary>
        /// Where element <paramref name="index" /> of an array starts. Element 0 is the variable's own offset.
        /// </summary>
        public int GetElementOffset(int index)
        {
            int elementOffset = m_Offset + index * m_Width.GetByteCount();

            return elementOffset;
        }

        /// <summary>
        /// What element <paramref name="index" /> starts at in a new game: its own default if it has one, otherwise
        /// the variable's.
        /// </summary>
        internal ulong GetDefault(int index)
        {
            ulong value = m_DefaultValue;

            for (int i = 0; i < m_ElementDefaults.Count; i++)
            {
                if (m_ElementDefaults[i].Index == index)
                {
                    value = m_ElementDefaults[i].Value;
                    break;
                }
            }

            return value;
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

    /// <summary>
    /// One element of an array variable that starts at its own value rather than the variable's default, so an
    /// array of hundreds can set a few without listing the rest.
    /// </summary>
    [Serializable]
    internal struct VariableElementDefault
    {
        [SerializeField]
        private int m_Index;

        [SerializeField]
        private ulong m_Value;

        public int   Index => m_Index;
        public ulong Value => m_Value;
    }
}
