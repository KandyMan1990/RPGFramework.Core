using System;
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
        private VariableDefinition[] m_Variables = Array.Empty<VariableDefinition>();

        [SerializeField]
        [HideInInspector]
        private uint m_LastVariableId;

        private Dictionary<string, VariableDefinition> m_ByName;

        IReadOnlyList<VariableDefinition> IVariableMap.Variables      => m_Variables;
        uint IVariableMap.                             LastVariableId => m_LastVariableId;

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

        bool IVariableMap.TryGetVariable(string varName, out VariableDefinition definition) => TryGetVariable(varName, out definition);

        /// <summary>
        /// Find a variable by the name it was authored under.
        /// </summary>
        private bool TryGetVariable(string varName, out VariableDefinition definition)
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

            for (int i = 0; i < m_Variables.Length; i++)
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
            if (m_ByName != null)
            {
                return;
            }

            m_ByName = new Dictionary<string, VariableDefinition>(m_Variables.Length);

            for (int i = 0; i < m_Variables.Length; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (string.IsNullOrWhiteSpace(variable.Name))
                {
                    continue;
                }

                m_ByName[variable.Name] = variable;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_ByName = null;
        }

        /// <summary>
        /// A new map starts holding the variables the framework requires, so an author fills in their defaults
        /// rather than having to discover their names.
        /// </summary>
        private void Reset()
        {
            m_Variables = Array.Empty<VariableDefinition>();

            AddMissingRequiredVariables();
        }

        /// <summary>
        /// Authoring only. Declares every required variable the map does not have yet, with the default it asks for,
        /// and returns how many were added.
        /// </summary>
        internal int AddMissingRequiredVariables()
        {
            int added = 0;

            RequiredVariable[] requiredVariables = RequiredVariables.FindAll();

            for (int i = 0; i < requiredVariables.Length; i++)
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

            Append(definition);

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

            Append(definition);

            return definition;
        }

        private void Append(VariableDefinition definition)
        {
            Array.Resize(ref m_Variables, m_Variables.Length + 1);

            m_Variables[m_Variables.Length - 1] = definition;
            m_ByName                            = null;
        }

        /// <summary>
        /// Authoring only. Gives every variable without an id, or sharing one with a variable before it — a copy made in
        /// the inspector — the next id the map has not used, does the same for each record's fields, and returns how
        /// many it gave. Ids are never reused, so a deleted variable's cannot reach a new one.
        /// </summary>
        internal int AssignMissingIds()
        {
            for (int i = 0; i < m_Variables.Length; i++)
            {
                VariableDefinition variable = m_Variables[i];

                if (variable.Id > m_LastVariableId)
                {
                    m_LastVariableId = variable.Id;
                }
            }

            HashSet<uint> seen     = new HashSet<uint>();
            int           assigned = 0;

            for (int i = 0; i < m_Variables.Length; i++)
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
        /// Authoring only. Where <paramref name="size" /> bytes at <paramref name="alignment" /> go in a bank: the first
        /// gap between its variables they fit, otherwise after the last. <paramref name="moving" />, a variable being
        /// relocated, counts its own bytes as free.
        /// </summary>
        internal int FindFreeOffset(MemoryBank bank, int alignment, int size, VariableDefinition moving = null)
        {
            List<VariableDefinition> occupied = new List<VariableDefinition>();

            for (int i = 0; i < m_Variables.Length; i++)
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
            for (int i = 0; i < m_Variables.Length; i++)
            {
                for (int j = 0; j < m_Variables.Length; j++)
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
        /// Authoring only. Every problem <see cref="VariableMapValidation" /> finds with this map, against the variables
        /// the framework requires and the modules a game can begin in. An empty list means the map is well formed.
        /// </summary>
        internal string[] Validate()
        {
            string[] problems = VariableMapValidation.Validate(m_Variables, m_LastVariableId, RequiredVariables.FindAll(), RequiredVariables.FindStartModules());

            return problems;
        }
#endif
    }
}