using System;
using System.Collections.Generic;
using RPGFramework.Core.Memory;
using UnityEditor;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    /// <summary>
    /// Variable maps edited as an author edits them, through the asset's serialized properties: emptied, and variables
    /// or fields deleted, duplicated, resized or given an element's own default. An edit makes the map's variables
    /// afresh, so a test finds one by name after it.
    /// </summary>
    internal static class MapAuthoring
    {
        private const string VARIABLES = "m_Variables";

        /// <summary>A new map, every variable deleted from its list: a new one is given the required variables.</summary>
        internal static VariableMapAsset Empty()
        {
            VariableMapAsset map   = ScriptableObject.CreateInstance<VariableMapAsset>();
            SerializedObject asset = new SerializedObject(map);

            asset.FindProperty(VARIABLES).ClearArray();
            asset.ApplyModifiedPropertiesWithoutUndo();

            return map;
        }

        internal static VariableDefinition Named(VariableMapAsset map, string name)
        {
            IReadOnlyList<VariableDefinition> variables = ((IVariableMap)map).Variables;
            VariableDefinition                found     = null;

            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Name == name)
                {
                    found = variables[i];
                }
            }

            return found;
        }

        internal static void Delete(VariableMapAsset map, string name)
        {
            SerializedObject asset = new SerializedObject(map);

            asset.FindProperty(VARIABLES).DeleteArrayElementAtIndex(IndexOf(asset, name));
            asset.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>A copy after it in the list, sharing its id and offset until the inspector sorts them out.</summary>
        internal static void Duplicate(VariableMapAsset map, string name, string copyName)
        {
            SerializedObject   asset     = new SerializedObject(map);
            SerializedProperty variables = asset.FindProperty(VARIABLES);
            int                index     = IndexOf(asset, name);

            variables.InsertArrayElementAtIndex(index);
            variables.GetArrayElementAtIndex(index + 1).FindPropertyRelative("m_Name").stringValue = copyName;
            asset.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>A copy of a record's field after it, sharing its id until the inspector sorts it out.</summary>
        internal static void DuplicateField(VariableMapAsset map, string name, int field, string copyName)
        {
            SerializedObject   asset  = new SerializedObject(map);
            SerializedProperty fields = Variable(asset, name).FindPropertyRelative("m_Fields");

            fields.InsertArrayElementAtIndex(field);
            fields.GetArrayElementAtIndex(field + 1).FindPropertyRelative("m_Name").stringValue = copyName;
            asset.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void SetCount(VariableMapAsset map, string name, int count)
        {
            SerializedObject asset = new SerializedObject(map);

            Variable(asset, name).FindPropertyRelative("m_Count").intValue = count;
            asset.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>The id as a map saved before ids, or a hand-edited one, holds it.</summary>
        internal static void SetId(VariableMapAsset map, string name, uint id)
        {
            SerializedObject asset = new SerializedObject(map);

            Variable(asset, name).FindPropertyRelative("m_Id").uintValue = id;
            asset.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// An element's own default, or for a record a field of one record's: the inspector's "Add Element" and "Add
        /// Record" boxes.
        /// </summary>
        internal static void AddElementDefault(VariableMapAsset map, string name, int index, string field, int fieldIndex, ulong value)
        {
            SerializedObject   asset    = new SerializedObject(map);
            SerializedProperty defaults = Variable(asset, name).FindPropertyRelative("m_ElementDefaults");

            defaults.InsertArrayElementAtIndex(defaults.arraySize);

            SerializedProperty added = defaults.GetArrayElementAtIndex(defaults.arraySize - 1);

            added.FindPropertyRelative("m_Index").intValue      = index;
            added.FindPropertyRelative("m_Field").stringValue   = field;
            added.FindPropertyRelative("m_FieldIndex").intValue = fieldIndex;
            added.FindPropertyRelative("m_Value").ulongValue    = value;
            asset.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty Variable(SerializedObject asset, string name)
        {
            SerializedProperty variable = asset.FindProperty(VARIABLES).GetArrayElementAtIndex(IndexOf(asset, name));

            return variable;
        }

        private static int IndexOf(SerializedObject asset, string name)
        {
            SerializedProperty variables = asset.FindProperty(VARIABLES);

            for (int i = 0; i < variables.arraySize; i++)
            {
                if (variables.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue == name)
                {
                    return i;
                }
            }

            throw new ArgumentException($"The map has no variable called '{name}'", nameof(name));
        }
    }
}
