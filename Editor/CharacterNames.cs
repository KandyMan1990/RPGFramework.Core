using System;
using System.Collections.Generic;
using RPGFramework.Core.Databases;
using UnityEditor;

namespace RPGFramework.Core.Editor
{
    /// <summary>The names of the characters in the project's character databases, for pickers that store a name's hash.</summary>
    internal static class CharacterNames
    {
        /// <summary>Every character in every character database in the project, each once, sorted.</summary>
        internal static List<string> Gather()
        {
            List<string> names = new List<string>();
            string[]     guids = AssetDatabase.FindAssets("t:" + nameof(CharacterDatabase));

            for (int i = 0; i < guids.Length; i++)
            {
                CharacterDatabase database = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(AssetDatabase.GUIDToAssetPath(guids[i]));

                if (database == null)
                {
                    continue;
                }

                IReadOnlyList<CharacterDefinition> characters = database.Characters;

                for (int j = 0; j < characters.Count; j++)
                {
                    string name = characters[j].Name;

                    if (!string.IsNullOrEmpty(name) && !names.Contains(name))
                    {
                        names.Add(name);
                    }
                }
            }

            names.Sort(StringComparer.Ordinal);

            return names;
        }
    }
}
