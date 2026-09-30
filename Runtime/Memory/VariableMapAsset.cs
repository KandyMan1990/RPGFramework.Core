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
    /// <b>Offsets are allocated by this asset, never typed by hand, and are never reused.</b> Allocation
    /// appends after the highest offset already in use, so deleting a variable leaves a permanent hole.
    /// That is deliberate: once a game has shipped, a saved file holds bytes at fixed offsets, and handing
    /// a freed offset to a new variable would make it silently read the old variable's data out of every
    /// existing save.<br /><br />
    /// It also sizes the banks, each exactly big enough for what is declared in it. A game binds it as
    /// <see cref="IVariableMap" />, <see cref="IMemoryServiceArgs" /> and <see cref="ITempMemoryArgs" />, so the banks
    /// and the variables read from them cannot come from different maps. Growing the map between releases is safe: a
    /// save written by an older build restores into the larger bank, and what lies past its end takes its defaults.
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
        public int GetRequiredBytes(MemoryBank bank)
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
        /// Authoring only. Declares every required variable the map does not have yet, with a default of zero,
        /// and returns how many were added.
        /// </summary>
        public int AddMissingRequiredVariables()
        {
            int added = 0;

            foreach (RequiredVariable required in RequiredVariables.FindAll())
            {
                if (TryGetVariable(required.Name, out VariableDefinition _))
                {
                    continue;
                }

                Allocate(required.Name, required.Bank, required.Width, 1, required.Description, 0);
                added++;
            }

            return added;
        }

        /// <summary>
        /// Authoring only. Appends a variable of <paramref name="count" /> values — one, or an array — at the next
        /// free naturally aligned offset in its bank and returns it. Never reuses a hole left by a deleted variable —
        /// see the note on this class.
        /// </summary>
        public VariableDefinition Allocate(string name, MemoryBank bank, VariableWidth width, int count, string description, ulong defaultValue)
        {
            int offset = GetNextOffset(bank, width.GetByteCount());

            VariableDefinition definition = new VariableDefinition(++m_LastVariableId, name, bank, width, count, offset, description, defaultValue);

            m_Variables.Add(definition);
            m_ByName = null;

            return definition;
        }

        /// <summary>
        /// Authoring only. As <see cref="Allocate" />, for <paramref name="count" /> records of
        /// <paramref name="fields" />, aligned to the widest field. Each field's default starts at zero.
        /// </summary>
        public VariableDefinition AllocateRecord(string name, MemoryBank bank, IReadOnlyList<VariableRecordField> fields, int count, string description)
        {
            int offset = GetNextOffset(bank, GetAlignment(fields));

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
            foreach (VariableDefinition variable in m_Variables)
            {
                if (variable.Id > m_LastVariableId)
                {
                    m_LastVariableId = variable.Id;
                }
            }

            HashSet<uint> seen     = new HashSet<uint>();
            int           assigned = 0;

            foreach (VariableDefinition variable in m_Variables)
            {
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
        public static int GetAlignment(IReadOnlyList<VariableRecordField> fields)
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
        public static bool IsScriptName(string name)
        {
            bool isScriptName = !string.IsNullOrEmpty(name) && !char.IsDigit(name[0]);

            if (isScriptName)
            {
                foreach (char c in name)
                {
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
        /// Authoring only. The offset <see cref="Allocate" /> would use for this bank and alignment: the first
        /// aligned offset at or after the end of the highest variable currently in the bank.
        /// </summary>
        public int GetNextOffset(MemoryBank bank, int alignment)
        {
            int highestEnd = GetRequiredBytes(bank);
            int remainder  = highestEnd % alignment;

            int offset = remainder == 0 ? highestEnd : highestEnd + (alignment - remainder);

            return offset;
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

            foreach (VariableDefinition variable in m_Variables)
            {
                if (variable.Id == 0 || variable.Id > m_LastVariableId || !ids.Add(variable.Id))
                {
                    problems.Add($"'{variable.Name}' has no id of its own. Open the map's inspector, which assigns one");
                }

                HashSet<ushort> fieldIds = new HashSet<ushort>();

                foreach (VariableRecordField field in variable.Fields)
                {
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

            foreach (VariableRecordField field in variable.Fields)
            {
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

            foreach (VariableElementDefault elementDefault in variable.ElementDefaults)
            {
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
            foreach (RequiredVariable required in RequiredVariables.FindAll())
            {
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

            foreach (IStartModule module in RequiredVariables.FindStartModules())
            {
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