using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// <see cref="IPersistentValues" /> for one variable map, its banks as long as the map's memory args make them, its
    /// save files written and read as the game's save service writes and reads them.
    /// </summary>
    public sealed class PersistentValues : IPersistentValues
    {
        private readonly PersistentValue[] m_Values;
        private readonly SaveFileFormat    m_Format;

        IReadOnlyList<PersistentValue> IPersistentValues.Values => m_Values;

        public PersistentValues(IVariableMap map, IMemoryServiceArgs memory)
        {
            m_Values = LayOut(map);
            m_Format = new SaveFileFormat(map, memory.PersistentBytes);
        }

        byte[] IPersistentValues.CreateDefaultBank()
        {
            return m_Format.DefaultBank();
        }

        ulong IPersistentValues.Read(byte[] bank, PersistentValue value)
        {
            ulong bits = 0;

            for (int i = 0; i < value.Width.GetByteCount(); i++)
            {
                bits |= (ulong)bank[value.Offset + i] << (8 * i);
            }

            return bits;
        }

        void IPersistentValues.Write(byte[] bank, PersistentValue value, ulong bits)
        {
            for (int i = 0; i < value.Width.GetByteCount(); i++)
            {
                bank[value.Offset + i] = (byte)(bits >> (8 * i));
            }
        }

        byte[] IPersistentValues.WriteSave(byte[] bank)
        {
            return m_Format.Write(new Dictionary<ulong, SectionBlob>(), bank);
        }

        bool IPersistentValues.TryReadSave(byte[] save, out byte[] bank)
        {
            return m_Format.Read(save, new Dictionary<ulong, SectionBlob>(), out bank) == SaveState.Loadable;
        }

        private static PersistentValue[] LayOut(IVariableMap map)
        {
            IReadOnlyList<VariableDefinition> variables = map.Variables;
            int                               count     = 0;

            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Bank == MemoryBank.Persistent)
                {
                    count += CountValues(variables[i]);
                }
            }

            PersistentValue[] values = new PersistentValue[count];
            int               next   = 0;

            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Bank == MemoryBank.Persistent)
                {
                    next = LayOut(variables[i], values, next);
                }
            }

            return values;
        }

        private static int CountValues(VariableDefinition variable)
        {
            if (!variable.IsRecord)
            {
                return variable.Count;
            }

            int perRecord = 0;

            for (int f = 0; f < variable.Fields.Count; f++)
            {
                perRecord += variable.Fields[f].Count;
            }

            return variable.Count * perRecord;
        }

        // Fills the variable's values from next, returning the index after its last.
        private static int LayOut(VariableDefinition variable, PersistentValue[] values, int next)
        {
            for (int index = 0; index < variable.Count; index++)
            {
                string element = variable.Count == 1 ? variable.Name : $"{variable.Name}[{index}]";
                int    offset  = variable.GetElementOffset(index);

                if (!variable.IsRecord)
                {
                    values[next++] = new PersistentValue(variable, element, index, null, 0, offset, variable.GetDefault(index));
                    continue;
                }

                IReadOnlyList<VariableRecordField> fields = variable.Fields;

                for (int f = 0; f < fields.Count; f++)
                {
                    VariableRecordField field = fields[f];
                    int                 size  = field.Width.GetByteCount();

                    for (int fieldIndex = 0; fieldIndex < field.Count; fieldIndex++)
                    {
                        string label = field.Count == 1 ? $"{element}.{field.Name}" : $"{element}.{field.Name}[{fieldIndex}]";

                        values[next++] = new PersistentValue(variable, label, index, field, fieldIndex, offset + fieldIndex * size, variable.GetDefault(index, field, fieldIndex));
                    }

                    offset += field.ByteCount;
                }
            }

            return next;
        }
    }
}
