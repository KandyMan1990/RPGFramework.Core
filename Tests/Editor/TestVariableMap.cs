using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests
{
    /// <summary>A variable map built in code, handing out ids as a map does unless a test gives its own.</summary>
    internal sealed class TestVariableMap : IVariableMap
    {
        private readonly List<VariableDefinition> m_Variables;

        private uint m_LastVariableId;

        internal TestVariableMap()
        {
            m_Variables      = new List<VariableDefinition>();
            m_LastVariableId = 0;
        }

        IReadOnlyList<VariableDefinition> IVariableMap.Variables      => m_Variables;
        uint IVariableMap.                             LastVariableId => m_LastVariableId;

        bool IVariableMap.TryGetVariable(string varName, out VariableDefinition definition)
        {
            definition = m_Variables.Find(variable => variable.Name == varName);

            bool found = definition != null;

            return found;
        }

        internal VariableDefinition this[int index] => m_Variables[index];

        internal TestVariableMap Add(string name, MemoryBank bank, VariableWidth width, int offset, ulong defaultValue, int count = 1, uint id = 0)
        {
            m_Variables.Add(new VariableDefinition(NextId(id), name, bank, width, count, offset, string.Empty, defaultValue));

            return this;
        }

        /// <summary>A record whose fields keep any ids the test gave them, the rest given as the inspector would.</summary>
        internal TestVariableMap AddRecord(string name, MemoryBank bank, int offset, int count, VariableRecordField[] fields, uint id = 0)
        {
            VariableDefinition record = new VariableDefinition(NextId(id), name, bank, fields, count, offset, string.Empty);

            record.AssignMissingFieldIds();
            m_Variables.Add(record);

            return this;
        }

        private uint NextId(uint id)
        {
            uint next = id != 0 ? id : m_LastVariableId + 1;

            if (next > m_LastVariableId)
            {
                m_LastVariableId = next;
            }

            return next;
        }
    }
}
