using System;
using System.Collections.Generic;
using RPGFramework.Core.Databases;
using UnityEditor;

namespace RPGFramework.Core.Editor
{
    /// <summary>The characters a script or a picker can name, for anything that stores a name's hash.</summary>
    public interface ICharacterNames
    {
        /// <summary>Every character in every character database in the project, each once, sorted.</summary>
        IReadOnlyList<string> Names { get; }
    }

    /// <summary><see cref="ICharacterNames" /> read from the project's character databases as it is made.</summary>
    public sealed class CharacterNames : ICharacterNames
    {
        private readonly List<string> m_Names;

        public CharacterNames()
        {
            m_Names = new List<string>();

            string[] guids = AssetDatabase.FindAssets("t:" + nameof(CharacterDatabase));

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

                    if (!string.IsNullOrEmpty(name) && !m_Names.Contains(name))
                    {
                        m_Names.Add(name);
                    }
                }
            }

            m_Names.Sort(StringComparer.Ordinal);
        }

        IReadOnlyList<string> ICharacterNames.Names => m_Names;
    }
}
