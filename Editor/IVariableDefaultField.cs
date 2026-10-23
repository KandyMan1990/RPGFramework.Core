using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// The value of one named variable, drawn as a choice instead of a number — a module from a list, a field by name
    /// rather than its hash — wherever a value is set outside a playthrough: its default in the Variable Map, or its
    /// value in another tool's asset. A package supplies one for each required variable whose value names something;
    /// they are found by type, so each needs a public parameterless constructor.
    /// </summary>
    public interface IVariableDefaultField
    {
        /// <summary>The name of the variable whose value this draws.</summary>
        string VariableName { get; }

        /// <summary>The field shown in place of the variable's number, writing the value as it changes.</summary>
        /// <param name="value">The value it shows first.</param>
        /// <param name="write">Writes a value chosen.</param>
        /// <param name="context">The other variables' values where this one is set, for a field that offers what one names.</param>
        VisualElement Create(ulong value, Action<ulong> write, IVariableValueContext context);
    }

    /// <summary>The variables' values where a value is being set: the map's defaults, or another tool's own values.</summary>
    public interface IVariableValueContext
    {
        /// <summary>The value of a single variable here, by name; zero for one there is none of.</summary>
        ulong ValueOf(string variableName);

        /// <summary>Calls <paramref name="changed" /> as the variable's value here changes, for as long as <paramref name="owner" /> is shown.</summary>
        void Watch(VisualElement owner, string variableName, Action changed);
    }

    internal static class VariableDefaultFields
    {
        private static Dictionary<string, IVariableDefaultField> m_ByName;

        internal static bool TryGet(string variableName, out IVariableDefaultField field)
        {
            if (m_ByName == null)
            {
                m_ByName = new Dictionary<string, IVariableDefaultField>();

                TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IVariableDefaultField>();

                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];

                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    IVariableDefaultField instance = (IVariableDefaultField)Activator.CreateInstance(type);
                    m_ByName[instance.VariableName] = instance;
                }
            }

            bool found = m_ByName.TryGetValue(variableName, out field);

            return found;
        }
    }
}
