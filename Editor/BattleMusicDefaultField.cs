using System;
using System.Collections.Generic;
using RPGFramework.Audio.Music;
using RPGFramework.Core.Memory;
using RPGFramework.Hashing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// The track every battle plays in a new game, until a script sets another, chosen by name from the project's music
    /// providers; the variable holds the name's hash, and None, zero.
    /// </summary>
    internal sealed class BattleMusicDefaultField : IVariableDefaultField
    {
        private const string NONE = "None";

        string IVariableDefaultField.VariableName => CoreVariables.BATTLE_MUSIC;

        VisualElement IVariableDefaultField.Create(SerializedProperty variable)
        {
            SerializedProperty defaultValue = variable.FindPropertyRelative(VariableDefinitionDrawer.DEFAULT_VALUE);
            List<string>       names        = GatherMusicNames();

            names.Insert(0, NONE);

            int chosen = defaultValue.ulongValue == 0 ? 0 : IndexOfHash(names, defaultValue.ulongValue);

            DropdownField field = new DropdownField("Battle music", names, Mathf.Max(0, chosen));

            if (chosen < 0)
            {
                field.SetValueWithoutNotify(string.Empty);
            }

            field.RegisterValueChangedCallback(e => VariableDefinitionDrawer.Write(defaultValue, names.IndexOf(e.newValue) == 0 ? 0UL : Fnv1a64.Hash(e.newValue)));

            return field;
        }

        // Every song in every music provider in the project, each once, sorted.
        private static List<string> GatherMusicNames()
        {
            List<string> names = new List<string>();
            string[]     guids = AssetDatabase.FindAssets("t:" + nameof(MusicAssetProvider));

            for (int i = 0; i < guids.Length; i++)
            {
                MusicAssetProvider provider = AssetDatabase.LoadAssetAtPath<MusicAssetProvider>(AssetDatabase.GUIDToAssetPath(guids[i]));

                if (provider == null)
                {
                    continue;
                }

                foreach (string name in provider.AssetNames)
                {
                    if (!names.Contains(name))
                    {
                        names.Add(name);
                    }
                }
            }

            names.Sort(StringComparer.Ordinal);

            return names;
        }

        // The index of the name with this hash, past None at 0; -1 when no song has it.
        private static int IndexOfHash(List<string> names, ulong hash)
        {
            for (int i = 1; i < names.Count; i++)
            {
                if (Fnv1a64.Hash(names[i]) == hash)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}