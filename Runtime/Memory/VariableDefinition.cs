using System;
using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// One named variable in a <see cref="VariableMapAsset" />: a single value, or an array of <see cref="Count" />
    /// values of its width laid end to end — or, when it has <see cref="Fields" />, one record or an array of records,
    /// each its fields packed end to end.<br /><br />
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
        [Tooltip("Storage width. Determines which IMemoryService accessor reads this variable. Unused by a record, whose fields have their own")]
        private VariableWidth m_Width;

        [SerializeField]
        [Tooltip("How many values of its width, or records, this variable holds. 1 is a single one; more makes an array, its elements end to end")]
        private int m_Count;

        [SerializeField]
        [Tooltip("A record's fields, in order, packed end to end. Empty for a value or an array of values")]
        private List<VariableRecordField> m_Fields = new List<VariableRecordField>();

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
        [Tooltip("Elements of an array, or fields of particular records, that start at something other than the default. Edit them through the Variable Map inspector")]
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

        public IReadOnlyList<VariableRecordField> Fields => m_Fields;

        public bool IsRecord => m_Fields.Count > 0;

        internal IReadOnlyList<VariableElementDefault> ElementDefaults => m_ElementDefaults;

        /// <summary>
        /// How many bytes one element takes: its width, or a record's fields end to end.
        /// </summary>
        public int ElementSize
        {
            get
            {
                int elementSize = IsRecord ? 0 : m_Width.GetByteCount();

                for (int i = 0; i < m_Fields.Count; i++)
                {
                    elementSize += m_Fields[i].ByteCount;
                }

                return elementSize;
            }
        }

        /// <summary>
        /// The first byte after this variable, i.e. <see cref="Offset" /> plus <see cref="ElementSize" /> for every element.
        /// </summary>
        public int EndOffset
        {
            get
            {
                int endOffset = m_Offset + ElementSize * m_Count;

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

        public VariableDefinition(string name, MemoryBank bank, IReadOnlyList<VariableRecordField> fields, int count, int offset, string description)
        {
            m_Name        = name;
            m_Bank        = bank;
            m_Fields      = new List<VariableRecordField>(fields);
            m_Count       = count;
            m_Offset      = offset;
            m_Description = description;
        }

        /// <summary>
        /// Where element <paramref name="index" /> of an array starts. Element 0 is the variable's own offset.
        /// </summary>
        public int GetElementOffset(int index)
        {
            int elementOffset = m_Offset + index * ElementSize;

            return elementOffset;
        }

        /// <summary>
        /// Find a record's field by the name scripts use, and where it starts within each record.
        /// </summary>
        public bool TryGetField(string fieldName, out VariableRecordField field, out int fieldOffset)
        {
            field       = null;
            fieldOffset = 0;

            bool found = false;

            for (int i = 0; i < m_Fields.Count && !found; i++)
            {
                if (m_Fields[i].Name == fieldName)
                {
                    field = m_Fields[i];
                    found = true;
                }
                else
                {
                    fieldOffset += m_Fields[i].ByteCount;
                }
            }

            return found;
        }

        /// <summary>
        /// What element <paramref name="index" /> starts at in a new game: its own default if it has one, otherwise
        /// the variable's.
        /// </summary>
        internal ulong GetDefault(int index)
        {
            ulong value = FindElementDefault(index, string.Empty, 0, m_DefaultValue);

            return value;
        }

        /// <summary>
        /// What value <paramref name="fieldIndex" /> of <paramref name="field" /> starts at in record
        /// <paramref name="index" />: that record's own default for it if it has one, otherwise the field's.
        /// </summary>
        internal ulong GetDefault(int index, VariableRecordField field, int fieldIndex)
        {
            ulong value = FindElementDefault(index, field.Name, fieldIndex, field.DefaultValue);

            return value;
        }

        private ulong FindElementDefault(int index, string fieldName, int fieldIndex, ulong fallback)
        {
            ulong value = fallback;

            for (int i = 0; i < m_ElementDefaults.Count; i++)
            {
                VariableElementDefault elementDefault = m_ElementDefaults[i];

                if (elementDefault.Index == index && elementDefault.Field == fieldName && elementDefault.FieldIndex == fieldIndex)
                {
                    value = elementDefault.Value;
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
    /// One field of a record variable, packed after the one before it. A count above 1 makes it an array inside the
    /// record, as a fixed buffer is inside a struct.
    /// </summary>
    [Serializable]
    public sealed class VariableRecordField
    {
        [SerializeField]
        [Tooltip("Name scripts use after the record, as in $characters[0].hp. Unique within the record")]
        private string m_Name;

        [SerializeField]
        [Tooltip("Storage width of each value")]
        private VariableWidth m_Width;

        [SerializeField]
        [Tooltip("How many values of its width. More than 1 makes an array inside the record")]
        private int m_Count;

        [SerializeField]
        [Tooltip("What every record starts with in a new game, held as the bytes it occupies. Edit it through the Variable Map inspector")]
        private ulong m_DefaultValue;

        public string        Name         => m_Name;
        public VariableWidth Width        => m_Width;
        public int           Count        => m_Count;
        public ulong         DefaultValue => m_DefaultValue;

        public int ByteCount
        {
            get
            {
                int byteCount = m_Width.GetByteCount() * m_Count;

                return byteCount;
            }
        }

        public VariableRecordField(string name, VariableWidth width, int count, ulong defaultValue)
        {
            m_Name         = name;
            m_Width        = width;
            m_Count        = count;
            m_DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// One value that starts at its own default rather than the one covering it: an element of an array, or a field
    /// of one record — and for a field that is itself an array, one of its elements — so an array of hundreds can set
    /// a few without listing the rest.
    /// </summary>
    [Serializable]
    internal struct VariableElementDefault
    {
        [SerializeField]
        private int m_Index;

        [SerializeField]
        private string m_Field;

        [SerializeField]
        private int m_FieldIndex;

        [SerializeField]
        private ulong m_Value;

        public int    Index      => m_Index;
        public string Field      => m_Field ?? string.Empty;
        public int    FieldIndex => m_FieldIndex;
        public ulong  Value      => m_Value;
    }
}
