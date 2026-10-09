using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// The default of one named variable, drawn as a choice instead of a number — a module from a list, a
    /// field by name rather than its hash. A package supplies one for each required variable whose value names
    /// something; the Variable Map inspector finds them by type, so each needs a public parameterless
    /// constructor.
    /// </summary>
    public interface IVariableDefaultField
    {
        string VariableName { get; }

        /// <param name="variable">The <c>VariableDefinition</c> in the map, so a field can read its siblings.</param>
        VisualElement Create(SerializedProperty variable);
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
