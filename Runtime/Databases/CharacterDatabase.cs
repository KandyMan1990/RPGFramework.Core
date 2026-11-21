using System;
using RPGFramework.Hashing;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace RPGFramework.Core.Databases
{
    /// <summary>
    /// The game's characters, each by name. Bound by the game's global installer as <see cref="ICharacterDatabase" />; the
    /// Variable Map offers its names for each <see cref="Memory.CoreVariables.ROSTER" /> record's character.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Core/Character Database", fileName = "Character Database")]
    public sealed class CharacterDatabase : ScriptableObject, ICharacterDatabase
    {
        [SerializeField]
        private CharacterDefinition[] m_Characters = Array.Empty<CharacterDefinition>();

        [NonSerialized]
        private ulong[] m_Hashes;

#if UNITY_EDITOR
        internal IReadOnlyList<CharacterDefinition> Characters => m_Characters;

        private void OnValidate()
        {
            m_Hashes = null;
        }
#endif

        bool ICharacterDatabase.TryGetDefinition(ulong character, out CharacterDefinition definition)
        {
            m_Hashes ??= HashNames();

            for (int i = 0; i < m_Hashes.Length; i++)
            {
                if (m_Hashes[i] == character)
                {
                    definition = m_Characters[i];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        private ulong[] HashNames()
        {
            ulong[] hashes = new ulong[m_Characters.Length];

            for (int i = 0; i < hashes.Length; i++)
            {
                hashes[i] = Fnv1a64.Hash(m_Characters[i].Name);
            }

            return hashes;
        }
    }
}
