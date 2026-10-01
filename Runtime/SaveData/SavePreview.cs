using System;
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

        /// <summary>
        /// Written by a newer version of the game, holding variables this one does not know. It cannot be loaded —
        /// that would drop them — though saving over it is the player's choice.
        /// </summary>
        public bool IsFromNewerVersion { get; }

        /// <summary>
        /// Its checksum or contents are wrong — a write cut short, or a file damaged since. It cannot be loaded, and its
        /// variables read their defaults.
        /// </summary>
        public bool IsDamaged { get; }

        public bool CanLoad => !IsFromNewerVersion && !IsDamaged;

        private readonly byte[]       m_Persistent;
        private readonly IVariableMap m_VariableMap;

        internal SavePreview(string fileName, DateTime lastWritten, byte[] persistent, IVariableMap variableMap, SaveState state)
        {
            FileName           = fileName;
            LastWritten        = lastWritten;
            IsFromNewerVersion = state == SaveState.FromNewerVersion;
            IsDamaged          = state == SaveState.Damaged;
            m_Persistent       = persistent;
            m_VariableMap      = variableMap;
        }

        /// <summary>
        /// A persistent variable as this save holds it. A variable added to the map since the save was written reads
        /// its default, as it would once the save was loaded.
        /// </summary>
        public T Read<T>(string variableName) where T : unmanaged
        {
            m_VariableMap.TryGetVariable(variableName, out VariableDefinition variable);

            T value = MemoryMarshal.Read<T>(m_Persistent.AsSpan(variable.Offset));

            return value;
        }
    }
}