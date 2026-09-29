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
    /// existing save.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Core/Variable Map", fileName = "VariableMap")]
    public sealed class VariableMapAsset : ScriptableObject, IVariableMap
    {
        [SerializeField]
        private List<VariableDefinition> m_Variables = new List<VariableDefinition>();

        private Dictionary<string, VariableDefinition> m_ByName;

        public IReadOnlyList<VariableDefinition> Variables => m_Variables;

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
        /// How many bytes a bank needs to hold every variable declared for it. This is the value to give
        /// <see cref="IMemoryServiceArgs" /> — see <see cref="VariableMapMemoryServiceArgs" />.
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
            int offset = GetNextOffset(bank, width);

            VariableDefinition definition = new VariableDefinition(name, bank, width, count, offset, description, defaultValue);

            m_Variables.Add(definition);
            m_ByName = null;

            return definition;
        }

        /// <summary>
        /// Authoring only. The offset <see cref="Allocate" /> would use for this bank and width: the first
        /// naturally aligned offset at or after the end of the highest variable currently in the bank.
        /// </summary>
        public int GetNextOffset(MemoryBank bank, VariableWidth width)
        {
            int highestEnd = GetRequiredBytes(bank);
            int alignment  = width.GetByteCount();
            int remainder  = highestEnd % alignment;

            int offset = remainder == 0 ? highestEnd : highestEnd + (alignment - remainder);

            return offset;
        }

        /// <summary>
        /// Authoring only. Returns a human-readable problem for every duplicate name, overlapping range,
        /// negative offset, missing name, count below one or element default outside its array, for a required
        /// variable the map lacks or declares differently, and for a start module default no module answers to.
        /// An empty list means the map is well formed.
        /// </summary>
        public List<string> Validate()
        {
            List<string>    problems = new List<string>();
            HashSet<string> seen     = new HashSet<string>();

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

                if (variable.Offset < 0)
                {
                    problems.Add($"'{variable.Name}' has a negative offset ({variable.Offset})");
                }

                if (variable.Count < 1)
                {
                    problems.Add($"'{variable.Name}' has a count of {variable.Count}, and a variable holds at least one value");
                }

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

        private static void ValidateElementDefaults(VariableDefinition variable, List<string> problems)
        {
            HashSet<int> indices = new HashSet<int>();

            foreach (VariableElementDefault elementDefault in variable.ElementDefaults)
            {
                if (elementDefault.Index < 0 || elementDefault.Index >= variable.Count)
                {
                    problems.Add($"'{variable.Name}' has a default for element [{elementDefault.Index}], outside its {variable.Count} element(s)");
                }

                if (!indices.Add(elementDefault.Index))
                {
                    problems.Add($"'{variable.Name}' has more than one default for element [{elementDefault.Index}]");
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