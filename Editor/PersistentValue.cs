using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// One value in the persistent bank: its variable, by name and permanent id, the element of an array or the record,
    /// and for a record the field, by its id, and the field's element — as a script reads it, <c>Party[2].hp</c> — with
    /// its width and what a new game starts it at.
    /// </summary>
    public readonly struct PersistentValue
    {
        public string        VariableName { get; }
        public string        Label        { get; }
        public uint          VariableId   { get; }
        public int           Index        { get; }
        public ushort        FieldId      { get; }
        public int           FieldIndex   { get; }
        public VariableWidth Width        { get; }
        public ulong         Default      { get; }

        /// <summary>The variable is this one value, neither an array nor a record, so a package's picker for it applies.</summary>
        public bool IsWholeVariable { get; }

        internal int Offset { get; }

        internal PersistentValue(VariableDefinition variable, string label, int index, VariableRecordField field, int fieldIndex, int offset, ulong defaultValue)
        {
            VariableName    = variable.Name;
            Label           = label;
            VariableId      = variable.Id;
            Index           = index;
            FieldId         = field == null ? (ushort)0 : field.Id;
            FieldIndex      = fieldIndex;
            Width           = field == null ? variable.Width : field.Width;
            Default         = defaultValue;
            IsWholeVariable = field == null && variable.Count == 1;
            Offset          = offset;
        }
    }
}
