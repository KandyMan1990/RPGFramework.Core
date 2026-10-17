using System;
using System.Collections.Generic;
using RPGFramework.Audio.Music;
using RPGFramework.Audio.Sfx;
using RPGFramework.Hashing;
using UnityEditor;

namespace RPGFramework.Core.Editor
{
    /// <summary>The names of the songs and sounds in the project's audio providers, for pickers that store a hash.</summary>
    internal static class ProviderNames
    {
        /// <summary>The first choice of a picker, standing for <c>0</c>.</summary>
        internal const string NONE = "None";

        /// <summary>Every song in every music provider in the project, each once, sorted.</summary>
        internal static List<string> Music()
        {
            List<string> names = Gather<MusicAssetProvider>(provider => provider.AssetNames);

            return names;
        }

        /// <summary>Every sound in every sound effect provider in the project, each once, sorted.</summary>
        internal static List<string> Sounds()
        {
            List<string> names = Gather<SfxAssetProvider>(provider => provider.AssetNames);

            return names;
        }

        /// <summary>The index of the name with this hash, past a None at 0; -1 when no name has it.</summary>
        internal static int IndexOfHash(List<string> names, ulong hash)
        {
            int index = -1;

            for (int i = 1; i < names.Count && index < 0; i++)
            {
                if (Fnv1a64.Hash(names[i]) == hash)
                {
                    index = i;
                }
            }

            return index;
        }

        private static List<string> Gather<T>(Func<T, IEnumerable<string>> namesOf) where T : UnityEngine.Object
        {
            List<string> names = new List<string>();
            string[]     guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            for (int i = 0; i < guids.Length; i++)
            {
                T provider = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));

                if (provider == null)
                {
                    continue;
                }

                foreach (string name in namesOf(provider))
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
    }
}