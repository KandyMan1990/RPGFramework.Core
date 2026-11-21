using System;
using RPGFramework.Localisation;
using UnityEngine;

namespace RPGFramework.Core.Databases
{
    /// <summary>One of the game's characters: the name scripts and the variable map name it by, its name as the player reads it, and its portrait.</summary>
    [Serializable]
    public sealed class CharacterDefinition
    {
        [SerializeField, Tooltip("What scripts and the variable map name the character by; its hash is what a record holds")]
        private string m_Name;

        [SerializeField, LocalisationKey, Tooltip("The character's name as the player reads it")]
        private string m_NameKey;

        [SerializeField, Tooltip("The picture the menus and the save slots show")]
        private Sprite m_Portrait;

        /// <summary>The key of the character's name as the player reads it, <c>Sheet/Key</c>.</summary>
        public string NameKey => m_NameKey;

        /// <summary>The picture the menus and the save slots show.</summary>
        public Sprite Portrait => m_Portrait;

        internal string Name => m_Name;
    }
}
