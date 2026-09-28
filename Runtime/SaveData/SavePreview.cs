using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using RPGFramework.Core.Memory;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// What a save holds, read from its file without loading it, so the game in progress is untouched — what a list of
    /// save slots shows.
    /// </summary>
    public sealed class SavePreview
    {
        public string   FileName    { get; }
        public DateTime LastWritten { get; }

        private readonly byte[]       m_Persistent;
        private readonly IVariableMap m_VariableMap;

        internal SavePreview(string fileName, DateTime lastWritten, byte[] persistent, IVariableMap variableMap)
        {
            FileName      = fileName;
            LastWritten   = lastWritten;
            m_Persistent  = persistent;
            m_VariableMap = variableMap;
        }

        /// <summary>
        /// A persistent variable as this save holds it. A variable added to the map since the save was written reads
        /// its default, as it would once the save was loaded.
        /// </summary>
        public T Read<T>(string variableName) where T : unmanaged
        {
            m_VariableMap.TryGetVariable(variableName, out VariableDefinition variable);

            T value = variable.EndOffset <= m_Persistent.Length
                          ? MemoryMarshal.Read<T>(m_Persistent.AsSpan(variable.Offset))
                          : ReadDefault<T>(variable.DefaultValue);

            return value;
        }

        private static T ReadDefault<T>(ulong defaultValue) where T : unmanaged
        {
            Span<byte> bytes = stackalloc byte[sizeof(ulong)];

            BinaryPrimitives.WriteUInt64LittleEndian(bytes, defaultValue);

            T value = MemoryMarshal.Read<T>(bytes);

            return value;
        }
    }
}