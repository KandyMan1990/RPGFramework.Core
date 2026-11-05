using System;
using System.Collections.Generic;
using System.Text;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Input
{
    /// <summary>
    /// The player's bindings as the Settings bank holds them, <see cref="CoreVariables.INPUT_BINDINGS" />: each record a
    /// binding's id and the path of the control it is bound to instead.
    /// </summary>
    internal sealed class InputBindingsStore
    {
        private readonly IMemoryService m_Memory;
        private readonly int            m_Count;
        private readonly int            m_Offset;
        private readonly int            m_RecordSize;
        private readonly int            m_BindingOffset;
        private readonly int            m_PathOffset;

        internal InputBindingsStore(IMemoryService memory, IVariableMap map)
        {
            map.TryGetVariable(CoreVariables.INPUT_BINDINGS, out VariableDefinition variable);

            m_Memory        = memory;
            m_Count         = variable.Count;
            m_Offset        = variable.GetElementOffset(0);
            m_RecordSize    = variable.GetElementOffset(1) - m_Offset;
            m_BindingOffset = FieldOffset(variable.Fields, CoreVariables.INPUT_BINDING);
            m_PathOffset    = FieldOffset(variable.Fields, CoreVariables.INPUT_BINDING_PATH);
        }

        internal int Capacity => m_Count;

        /// <summary>The record's binding and path; false for an unused record.</summary>
        internal bool TryRead(int index, out Guid binding, out string path)
        {
            byte[] id = new byte[CoreVariables.INPUT_BINDING_BYTES];

            for (int i = 0; i < id.Length; i++)
            {
                id[i] = Read(index, m_BindingOffset + i);
            }

            binding = new Guid(id);

            byte[] text   = new byte[CoreVariables.INPUT_BINDING_PATH_BYTES];
            int    length = 0;

            while (length < text.Length && (text[length] = Read(index, m_PathOffset + length)) != 0)
            {
                length++;
            }

            path = Encoding.UTF8.GetString(text, 0, length);

            return binding != Guid.Empty;
        }

        /// <summary>Writes a record; false, writing nothing, for a path longer than a record holds.</summary>
        internal bool Write(int index, Guid binding, string path)
        {
            byte[] text = Encoding.UTF8.GetBytes(path);

            if (text.Length > CoreVariables.INPUT_BINDING_PATH_BYTES)
            {
                return false;
            }

            byte[] id = binding.ToByteArray();

            for (int i = 0; i < id.Length; i++)
            {
                Write(index, m_BindingOffset + i, id[i]);
            }

            for (int i = 0; i < CoreVariables.INPUT_BINDING_PATH_BYTES; i++)
            {
                Write(index, m_PathOffset + i, i < text.Length ? text[i] : (byte)0);
            }

            return true;
        }

        internal void Clear(int index)
        {
            for (int i = 0; i < m_RecordSize; i++)
            {
                Write(index, i, 0);
            }
        }

        private byte Read(int index, int offset)
        {
            return m_Memory.ReadByte(MemoryBank.Settings, (ushort)(m_Offset + index * m_RecordSize + offset));
        }

        private void Write(int index, int offset, byte value)
        {
            m_Memory.WriteByte(MemoryBank.Settings, (ushort)(m_Offset + index * m_RecordSize + offset), value);
        }

        private static int FieldOffset(IReadOnlyList<VariableRecordField> fields, string name)
        {
            int offset = 0;

            for (int i = 0; i < fields.Count && fields[i].Name != name; i++)
            {
                offset += fields[i].ByteCount;
            }

            return offset;
        }
    }
}
