using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// The authoring layer for the memory banks: a named, described, tool-allocated map of what lives
    /// at which byte offset.<br /><br />
    /// A memory bank on its own is an anonymous <c>byte[]</c>. This asset is what makes it readable —
    /// it is the schema that authoring tools resolve names against, and the record of which bytes are
    /// already spoken for.<br /><br />
    /// <b>Offsets are allocated by this asset, never typed by hand.</b> A new variable fills the first gap in its bank
    /// it fits, and a variable that grows into the next is moved. Saves record where each value was and are matched on
    /// permanent ids, so a value follows its variable wherever it goes, and one in a gap a deleted variable left is
    /// never read as the new one's.<br /><br />
    /// It also sizes the banks, each exactly big enough for what is declared in it. A game binds it as
    /// <see cref="IVariableMap" />, <see cref="IMemoryServiceArgs" /> and <see cref="ITempMemoryArgs" />, so the banks
    /// and the variables read from them cannot come from different maps.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Core/Variable Map", fileName = "VariableMap")]
    public sealed class VariableMapAsset : ScriptableObject, IVariableMap, IMemoryServiceArgs, ITempMemoryArgs
    {
        [SerializeField]
        private List<VariableDefinition> m_Variables = new List<VariableDefinition>();

        [SerializeField]
        [HideInInspector]
        private uint m_LastVariableId;

        private Dictionary<string, VariableDefinition> m_ByName;

        public IReadOnlyList<VariableDefinition> Variables => m_Variables;

        uint IVariableMap.LastVariableId => m_LastVariableId;

        int IMemoryServiceArgs.PersistentBytes
        {
            get
            {
                int persistentBytes = GetRequiredBytes(MemoryBank.Persistent);

                return persistentBytes;
            }
        }

        int IMemoryServiceArgs.SessionBytes
        {
            get
            {
                int sessionBytes = GetRequiredBytes(MemoryBank.Session);

                return sessionBytes;
            }
        }

        int ITempMemoryArgs.TempBytes
        {
            get
            {
                int tempBytes = GetRequiredBytes(MemoryBank.Temp);

                return tempBytes;
            }
        }

        /// <summary>
        /// Find a variable by the name it was authored under.
        /// </summary>
        public bool TryGetVariable(string varName, out VariableDefinition definition)
        {
            EnsureLookup();

            bool found = m_ByName.TryGetValue(varName, out definition);

            return found;
        }

        /// <summary>
        /// How many bytes a bank needs to hold every variable declared for it, which is the size it is given.
        /// </summary>
        internal int GetRequiredBytes(MemoryBank bank)
        {
            int requiredBytes = 0;

            for (int i = 0; i < m_Variables.Count; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (variable.Bank != bank)
                {
                    continue;
                }

                if (variable.EndOffset > requiredBytes)
                {
                    requiredBytes = variable.EndOffset;
                }
            }

            return requiredBytes;
        }

        private void EnsureLookup()
        {
            if (m_ByName != null && m_ByName.Count == m_Variables.Count)
            {
                return;
            }

            m_ByName = new Dictionary<string, VariableDefinition>(m_Variables.Count);

            for (int i = 0; i < m_Variables.Count; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (string.IsNullOrWhiteSpace(variable.Name))
                {
                    continue;
                }

                m_ByName[variable.Name] = variable;
            }
        }

        private void OnValidate()
        {
            m_ByName = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// A new map starts holding the variables the framework requires, so an author fills in their defaults
        /// rather than having to discover their names.
        /// </summary>
        private void Reset()
        {
            m_Variables.Clear();

            AddMissingRequiredVariables();
        }

        /// <summary>
        /// Authoring only. Declares every required variable the map does not have yet, with the default it asks for,
        /// and returns how many were added.
        /// </summary>
        internal int AddMissingRequiredVariables()
        {
            int added = 0;

            List<RequiredVariable> requiredVariables = RequiredVariables.FindAll();

            for (int i = 0; i < requiredVariables.Count; i++)
            {
                RequiredVariable required = requiredVariables[i];

                if (TryGetVariable(required.Name, out VariableDefinition _))
                {
                    continue;
                }

                Allocate(required.Name, required.Bank, required.Width, 1, required.Description, required.DefaultValue);
                added++;
            }

            return added;
        }

        /// <summary>
        /// Authoring only. Appends a variable of <paramref name="count" /> values — one, or an array — at the next
        /// free naturally aligned offset in its bank and returns it — the first gap it fits, otherwise the end.
        /// </summary>
        internal VariableDefinition Allocate(string name, MemoryBank bank, VariableWidth width, int count, string description, ulong defaultValue)
        {
            int offset = FindFreeOffset(bank, width.GetByteCount(), width.GetByteCount() * count);

            VariableDefinition definition = new VariableDefinition(++m_LastVariableId, name, bank, width, count, offset, description, defaultValue);

            m_Variables.Add(definition);
            m_ByName = null;

            return definition;
        }

        /// <summary>
        /// Authoring only. As <see cref="Allocate" />, for <paramref name="count" /> records of
        /// <paramref name="fields" />, aligned to the widest field. Each field's default starts at zero.
        /// </summary>
        internal VariableDefinition AllocateRecord(string name, MemoryBank bank, IReadOnlyList<VariableRecordField> fields, int count, string description)
        {
            int recordSize = 0;

            for (int i = 0; i < fields.Count; i++)
            {
                VariableRecordField field = fields[i];

                recordSize += field.ByteCount;
            }

            int offset = FindFreeOffset(bank, GetAlignment(fields), recordSize * count);

            VariableDefinition definition = new VariableDefinition(++m_LastVariableId, name, bank, fields, count, offset, description);
            definition.AssignMissingFieldIds();

            m_Variables.Add(definition);
            m_ByName = null;

            return definition;
        }

        /// <summary>
        /// Authoring only. Gives every variable without an id, or sharing one with a variable before it — a copy made in
        /// the inspector — the next id the map has not used, does the same for each record's fields, and returns how
        /// many it gave. Ids are never reused, so a deleted variable's cannot reach a new one.
        /// </summary>
        internal int AssignMissingIds()
        {
            for (int i = 0; i < m_Variables.Count; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (variable.Id > m_LastVariableId)
                {
                    m_LastVariableId = variable.Id;
                }
            }

            HashSet<uint> seen     = new HashSet<uint>();
            int           assigned = 0;

            for (int i = 0; i < m_Variables.Count; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (variable.Id == 0 || !seen.Add(variable.Id))
                {
                    variable.AssignId(++m_LastVariableId);
                    seen.Add(variable.Id);
                    assigned++;
                }

                assigned += variable.AssignMissingFieldIds();
            }

            return assigned;
        }

        /// <summary>
        /// Authoring only. What a record of <paramref name="fields" /> is aligned to: its widest field, as a struct is.
        /// </summary>
        internal static int GetAlignment(IReadOnlyList<VariableRecordField> fields)
        {
            int alignment = 1;

            for (int i = 0; i < fields.Count; i++)
            {
                int fieldAlignment = fields[i].Width.GetByteCount();

                if (fieldAlignment > alignment)
                {
                    alignment = fieldAlignment;
                }
            }

            return alignment;
        }

        /// <summary>
        /// Authoring only. Letters, digits and underscores, not starting with a digit — what a variable or a record
        /// field can be called, since scripts name them in text beside <c>[</c> and <c>.</c>.
        /// </summary>
        internal static bool IsScriptName(string name)
        {
            bool isScriptName = !string.IsNullOrEmpty(name) && !char.IsDigit(name[0]);

            if (isScriptName)
            {
                for (int i = 0; i < name.Length; i++)
                {
                    char c = name[i];

                    if (!char.IsLetterOrDigit(c) && c != '_')
                    {
                        isScriptName = false;
                        break;
                    }
                }
            }

            return isScriptName;
        }

        /// <summary>
        /// Authoring only. Where <paramref name="size" /> bytes at <paramref name="alignment" /> go in a bank: the first
        /// gap between its variables they fit, otherwise after the last. <paramref name="moving" />, a variable being
        /// relocated, counts its own bytes as free.
        /// </summary>
        internal int FindFreeOffset(MemoryBank bank, int alignment, int size, VariableDefinition moving = null)
        {
            List<VariableDefinition> occupied = new List<VariableDefinition>();

            for (int i = 0; i < m_Variables.Count; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (variable.Bank == bank && variable != moving)
                {
                    occupied.Add(variable);
                }
            }

            occupied.Sort((a, b) => a.Offset.CompareTo(b.Offset));

            int offset = 0;

            for (int i = 0; i < occupied.Count; i++)
            {
                VariableDefinition variable = occupied[i];

                if (offset + size <= variable.Offset)
                {
                    break;
                }

                offset = Align(Mathf.Max(offset, variable.EndOffset), alignment);
            }

            return offset;
        }

        /// <summary>
        /// Authoring only. Moves every variable that overlaps another to the first gap it fits, and returns how many it
        /// moved — a variable that grew into its neighbour, a copy made in the list, one moved onto another's bytes in
        /// a different bank. Of two that overlap, the one starting later moves (the later in the list, on a tie), so the
        /// one that grew stays where it is, and a state undo restores, which has no overlaps, is left alone. Saves follow
        /// the moved variable by its id.
        /// </summary>
        internal int RelocateOverlapping()
        {
            int moved = 0;

            VariableDefinition overlapping = FindLaterOverlapping();

            while (overlapping != null)
            {
                int alignment = overlapping.IsRecord ? GetAlignment(overlapping.Fields) : overlapping.Width.GetByteCount();
                int size      = overlapping.EndOffset - overlapping.Offset;

                overlapping.MoveTo(FindFreeOffset(overlapping.Bank, alignment, size, overlapping));
                moved++;

                overlapping = FindLaterOverlapping();
            }

            return moved;
        }

        private VariableDefinition FindLaterOverlapping()
        {
            for (int i = 0; i < m_Variables.Count; i++)
            {
                for (int j = 0; j < m_Variables.Count; j++)
                {
                    VariableDefinition variable = m_Variables[i];
                    VariableDefinition other    = m_Variables[j];

                    bool later = variable.Offset > other.Offset || (variable.Offset == other.Offset && i > j);

                    if (i != j && later && variable.Overlaps(other))
                    {
                        return variable;
                    }
                }
            }

            return null;
        }

        private static int Align(int offset, int alignment)
        {
            int remainder = offset % alignment;
            int aligned   = remainder == 0 ? offset : offset + (alignment - remainder);

            return aligned;
        }

        /// <summary>
        /// Authoring only. Returns a human-readable problem for every duplicate name, overlapping range,
        /// negative offset, missing name, count below one or element default outside its array, for a name scripts
        /// cannot use, for a record field that is misnamed, repeated or empty, for an id that is missing or shared, for
        /// a required variable the map lacks or declares differently, and for a start module default no module answers
        /// to. An empty list means the map is well formed.
        /// </summary>
        public List<string> Validate()
        {
            List<string>    problems = new List<string>();
            HashSet<string> seen     = new HashSet<string>();

            ValidateIds(problems);

            for (int i = 0; i < m_Variables.Count; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (string.IsNullOrWhiteSpace(variable.Name))
                {
                    problems.Add($"[{i}] has no name");
                    continue;
                }

                if (!seen.Add(variable.Name))
                {
                    problems.Add($"'{variable.Name}' is declared more than once");
                }

                if (!IsScriptName(variable.Name))
                {
                    problems.Add($"'{variable.Name}' is not a name scripts can use: letters, digits and underscores, not starting with a digit");
                }

                if (variable.Offset < 0)
                {
                    problems.Add($"'{variable.Name}' has a negative offset ({variable.Offset})");
                }

                if (variable.Count < 1)
                {
                    problems.Add($"'{variable.Name}' has a count of {variable.Count}, and a variable holds at least one value");
                }

                ValidateRecordFields(variable, problems);
                ValidateElementDefaults(variable, problems);

                for (int j = i + 1; j < m_Variables.Count; j++)
                {
                    VariableDefinition other = m_Variables[j];

                    if (variable.Overlaps(other))
                    {
                        problems.Add($"'{variable.Name}' ({variable.Bank} {variable.Offset}..{variable.EndOffset}) overlaps '{other.Name}' ({other.Offset}..{other.EndOffset})");
                    }
                }
            }

            ValidateRequiredVariables(problems);

            return problems;
        }

        private void ValidateIds(List<string> problems)
        {
            HashSet<uint> ids = new HashSet<uint>();

            for (int i = 0; i < m_Variables.Count; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (variable.Id == 0 || variable.Id > m_LastVariableId || !ids.Add(variable.Id))
                {
                    problems.Add($"'{variable.Name}' has no id of its own. Open the map's inspector, which assigns one");
                }

                HashSet<ushort> fieldIds = new HashSet<ushort>();

                for (int j = 0; j < variable.Fields.Count; j++)
                {
                    VariableRecordField field = variable.Fields[j];

                    if (field.Id == 0 || !fieldIds.Add(field.Id))
                    {
                        problems.Add($"'{variable.Name}.{field.Name}' has no id of its own. Open the map's inspector, which assigns one");
                    }
                }
            }
        }

        private static void ValidateRecordFields(VariableDefinition variable, List<string> problems)
        {
            HashSet<string> names = new HashSet<string>();

            for (int i = 0; i < variable.Fields.Count; i++)
            {
                VariableRecordField field = variable.Fields[i];

                if (!IsScriptName(field.Name))
                {
                    problems.Add($"'{variable.Name}' has a field called '{field.Name}', which scripts cannot use: letters, digits and underscores, not starting with a digit");
                }

                if (!names.Add(field.Name))
                {
                    problems.Add($"'{variable.Name}' has more than one field called '{field.Name}'");
                }

                if (field.Count < 1)
                {
                    problems.Add($"'{variable.Name}.{field.Name}' has a count of {field.Count}, and a field holds at least one value");
                }
            }
        }

        private static void ValidateElementDefaults(VariableDefinition variable, List<string> problems)
        {
            HashSet<string> described = new HashSet<string>();

            for (int i = 0; i < variable.ElementDefaults.Count; i++)
            {
                VariableElementDefault elementDefault = variable.ElementDefaults[i];

                string where = variable.IsRecord
                                   ? $"[{elementDefault.Index}].{elementDefault.Field}[{elementDefault.FieldIndex}]"
                                   : $"[{elementDefault.Index}]";

                if (elementDefault.Index < 0 || elementDefault.Index >= variable.Count)
                {
                    problems.Add($"'{variable.Name}' has a default for {where}, outside its {variable.Count} element(s)");
                }

                if (!variable.IsRecord && elementDefault.Field.Length > 0)
                {
                    problems.Add($"'{variable.Name}' is not a record, and has a default for a field called '{elementDefault.Field}'");
                }

                if (variable.IsRecord)
                {
                    if (!variable.TryGetField(elementDefault.Field, out VariableRecordField field, out int _))
                    {
                        problems.Add($"'{variable.Name}' has a default for {where}, and has no field called '{elementDefault.Field}'");
                    }
                    else if (elementDefault.FieldIndex < 0 || elementDefault.FieldIndex >= field.Count)
                    {
                        problems.Add($"'{variable.Name}' has a default for {where}, outside the field's {field.Count} element(s)");
                    }
                }

                if (!described.Add(where))
                {
                    problems.Add($"'{variable.Name}' has more than one default for {where}");
                }
            }
        }

        private void ValidateRequiredVariables(List<string> problems)
        {
            List<RequiredVariable> requiredVariables = RequiredVariables.FindAll();

            for (int i = 0; i < requiredVariables.Count; i++)
            {
                RequiredVariable required = requiredVariables[i];

                if (!TryGetVariable(required.Name, out VariableDefinition variable))
                {
                    problems.Add($"'{required.Name}' is required by the framework and is not declared — use Add Missing Required Variables. {required.Description}");
                    continue;
                }

                if (variable.Bank != required.Bank || variable.Width != required.Width)
                {
                    problems.Add($"'{required.Name}' is declared as {variable.Bank} {variable.Width}, but the framework reads it as {required.Bank} {required.Width}");
                }

                if (variable.Count != 1)
                {
                    problems.Add($"'{required.Name}' is declared as an array of {variable.Count}, but the framework reads it as a single value");
                }

                if (variable.IsRecord)
                {
                    problems.Add($"'{required.Name}' is declared as a record, but the framework reads it as a single {required.Width}");
                }
            }

            if (!TryGetVariable(CoreVariables.CURRENT_MODULE, out VariableDefinition currentModule))
            {
                return;
            }

            byte startModule = (byte)currentModule.DefaultValue;

            List<IStartModule> modules = RequiredVariables.FindStartModules();

            for (int i = 0; i < modules.Count; i++)
            {
                IStartModule module = modules[i];

                if (module.ModuleId == startModule)
                {
                    return;
                }
            }

            problems.Add($"'{CoreVariables.CURRENT_MODULE}' defaults to module [{startModule}], which is not a module a game can begin in. Choose one in the Variable Map inspector");
        }
#endif
    }
}