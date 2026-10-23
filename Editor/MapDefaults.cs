using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>The Variable Map's defaults, as the values a default field sets its own beside.</summary>
    internal sealed class MapDefaults : IVariableValueContext
    {
        private readonly SerializedObject m_Map;

        internal MapDefaults(SerializedObject map)
        {
            m_Map = map;
        }

        ulong IVariableValueContext.ValueOf(string variableName)
        {
            SerializedProperty value = FindDefault(variableName);

            return value == null ? 0UL : value.ulongValue;
        }

        void IVariableValueContext.Watch(VisualElement owner, string variableName, Action changed)
        {
            SerializedProperty value = FindDefault(variableName);

            if (value != null)
            {
                owner.TrackPropertyValue(value, _ => changed());
            }
        }

        private SerializedProperty FindDefault(string variableName)
        {
            SerializedProperty variables = m_Map.FindProperty("m_Variables");

            for (int i = 0; i < variables.arraySize; i++)
            {
                SerializedProperty variable = variables.GetArrayElementAtIndex(i);

                if (variable.FindPropertyRelative("m_Name").stringValue == variableName)
                {
                    return variable.FindPropertyRelative(VariableDefinitionDrawer.DEFAULT_VALUE);
                }
            }

            return null;
        }
    }
}
