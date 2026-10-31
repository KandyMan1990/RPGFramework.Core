using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.InteropServices;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

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
        /// A persistent value as this save holds it, named as scripts name it: a variable, an element of an array
        /// (<c>Items[3]</c>), or a record's field (<c>Party[0].Level</c>, and <c>Party[0].Stats[2]</c> for a field that is
        /// an array). A variable added to the map since the save was written reads its default, as it would once the
        /// save was loaded.
        /// </summary>
        public T Read<T>(string path) where T : unmanaged
        {
            int offset = Locate(path, out VariableWidth _);

            return MemoryMarshal.Read<T>(m_Persistent.AsSpan(offset));
        }

        /// <summary>
        /// As <see cref="Read{T}" />, the value as a number whatever its width, a bool as 1 or 0. A <c>ULong</c> above
        /// 2^53, such as a hash, loses precision, so read one with <see cref="Read{T}" />.
        /// </summary>
        public double ReadNumber(string path)
        {
            int offset = Locate(path, out VariableWidth width);

            ReadOnlySpan<byte> bytes = m_Persistent.AsSpan(offset);

            return width switch
                   {
                       VariableWidth.Bool   => bytes[0] != 0 ? 1 : 0,
                       VariableWidth.SByte  => (sbyte)bytes[0],
                       VariableWidth.Byte   => bytes[0],
                       VariableWidth.Short  => BinaryPrimitives.ReadInt16LittleEndian(bytes),
                       VariableWidth.UShort => BinaryPrimitives.ReadUInt16LittleEndian(bytes),
                       VariableWidth.Int    => BinaryPrimitives.ReadInt32LittleEndian(bytes),
                       VariableWidth.UInt   => BinaryPrimitives.ReadUInt32LittleEndian(bytes),
                       VariableWidth.Long   => BinaryPrimitives.ReadInt64LittleEndian(bytes),
                       VariableWidth.ULong  => BinaryPrimitives.ReadUInt64LittleEndian(bytes),
                       _                    => BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(bytes))
                   };
        }

        private int Locate(string path, out VariableWidth width)
        {
            int    dot   = path.IndexOf('.');
            string name  = SplitIndex(dot < 0 ? path : path.Substring(0, dot), path, out int index);
            string field = dot < 0 ? null : path.Substring(dot + 1);

            m_VariableMap.TryGetVariable(name, out VariableDefinition variable);
#if UNITY_EDITOR
            CheckVariable(path, name, index, field, variable);
#endif
            int offset = variable.GetElementOffset(index);

            if (field == null)
            {
                width = variable.Width;

                return offset;
            }

            string fieldName = SplitIndex(field, path, out int fieldIndex);
#if UNITY_EDITOR
            CheckField(path, name, variable, fieldName, fieldIndex);
#endif
            int next = 0;

            while (variable.Fields[next].Name != fieldName)
            {
                offset += variable.Fields[next].ByteCount;
                next++;
            }

            width = variable.Fields[next].Width;

            return offset + fieldIndex * width.GetByteCount();
        }

        private static string SplitIndex(string part, string path, out int index)
        {
            int open = part.IndexOf('[');

            index = 0;

            if (open < 0)
            {
                return part;
            }

#if UNITY_EDITOR
            if (part[part.Length - 1] != ']' || !int.TryParse(part.AsSpan(open + 1, part.Length - open - 2), NumberStyles.None, CultureInfo.InvariantCulture, out index))
            {
                throw new ArgumentException($"{nameof(SavePreview)} '{path}': an index is a whole number in brackets, as in Items[3]");
            }
#endif
            index = int.Parse(part.AsSpan(open + 1, part.Length - open - 2), NumberStyles.None, CultureInfo.InvariantCulture);

            return part.Substring(0, open);
        }

#if UNITY_EDITOR
        // The names come from authored text that nothing checks before it is read, so in the editor a name that misses
        // says what; a game that ships has every name right.
        private static void CheckVariable(string path, string name, int index, string field, VariableDefinition variable)
        {
            if (variable == null || variable.Bank != MemoryBank.Persistent)
            {
                throw new ArgumentException($"{nameof(SavePreview)} '{path}': a save holds no variable '{name}', only the persistent ones the map declares");
            }

            if (index >= variable.Count)
            {
                throw new ArgumentException($"{nameof(SavePreview)} '{path}': '{name}' holds {variable.Count} element(s)");
            }

            if (!variable.IsRecord && field != null)
            {
                throw new ArgumentException($"{nameof(SavePreview)} '{path}': '{name}' is not a record, so it has no field '{field}'");
            }

            if (variable.IsRecord && field == null)
            {
                throw new ArgumentException($"{nameof(SavePreview)} '{path}': '{name}' is a record, so name one of its fields, as in {name}[0].{variable.Fields[0].Name}");
            }
        }

        private static void CheckField(string path, string name, VariableDefinition variable, string fieldName, int fieldIndex)
        {
            for (int i = 0; i < variable.Fields.Count; i++)
            {
                VariableRecordField field = variable.Fields[i];

                if (field.Name != fieldName)
                {
                    continue;
                }

                if (fieldIndex >= field.Count)
                {
                    throw new ArgumentException($"{nameof(SavePreview)} '{path}': '{fieldName}' holds {field.Count} element(s)");
                }

                return;
            }

            throw new ArgumentException($"{nameof(SavePreview)} '{path}': '{name}' has no field called '{fieldName}'");
        }
#endif
    }
}