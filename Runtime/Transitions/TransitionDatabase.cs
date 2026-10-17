using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// The game's transitions, found by the hash of their names: what a router's change, a script or a module's own data
    /// names when it asks for one.
    /// </summary>
    public interface ITransitionDatabase
    {
        /// <summary>The transition named by <paramref name="nameHash" />.</summary>
        TransitionAsset Get(ulong nameHash);
    }

    [CreateAssetMenu(menuName = "RPG Framework/Transitions/Transition Database", fileName = "TransitionDatabase")]
    public sealed class TransitionDatabase : ScriptableObject, ITransitionDatabase
    {
        [SerializeField]
        private TransitionAsset[] m_Transitions = Array.Empty<TransitionAsset>();

        private Dictionary<ulong, TransitionAsset> m_ByNameHash;

#if UNITY_EDITOR
        /// <summary>Every transition it holds, for an editor offering their names.</summary>
        public IReadOnlyList<TransitionAsset> Transitions => m_Transitions;

        // As it is edited; a player only ever loads it.
        private void OnValidate()
        {
            BuildLookup();
        }
#endif

        private void OnEnable()
        {
            BuildLookup();
        }

        TransitionAsset ITransitionDatabase.Get(ulong nameHash)
        {
            TransitionAsset transition = m_ByNameHash[nameHash];

            return transition;
        }

        private void BuildLookup()
        {
            m_ByNameHash = new Dictionary<ulong, TransitionAsset>(m_Transitions.Length);

            for (int i = 0; i < m_Transitions.Length; i++)
            {
                if (m_Transitions[i] != null)
                {
                    m_ByNameHash[m_Transitions[i].NameHash] = m_Transitions[i];
                }
            }
        }
    }
}