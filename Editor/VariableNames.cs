using System;
using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using UnityEditor;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// The names of the single variables of one width a map holds outside Temp, ordered by name: what a
    /// <see cref="VariableNameAttribute" /> offers.
    /// </summary>
    internal static class VariableNames
    {
        internal static string[] Of(IVariableMap map, VariableWidth width)
        {
            IReadOnlyList<VariableDefinition> variables = map.Variables;
            string[]                          names     = new string[variables.Count];
            int                               count     = 0;

            for (int i = 0; i < variables.Count; i++)
            {
                VariableDefinition variable = variables[i];

                if (variable.Width == width && variable.Count == 1 && !variable.IsRecord && variable.Bank != MemoryBank.Temp)
                {
                    names[count++] = variable.Name;
                }
            }

            string[] of = new string[count];

            Array.Copy(names, of, count);
            Array.Sort(of, string.CompareOrdinal);

            return of;
        }

        // Every map's, once each: a project normally holds one.
        internal static string[] InProject(VariableWidth width)
        {
            string[]     guids = AssetDatabase.FindAssets("t:" + nameof(VariableMapAsset));
            List<string> names = new List<string>();

            for (int i = 0; i < guids.Length; i++)
            {
                VariableMapAsset map = AssetDatabase.LoadAssetAtPath<VariableMapAsset>(AssetDatabase.GUIDToAssetPath(guids[i]));
                string[]         of  = Of(map, width);

                for (int j = 0; j < of.Length; j++)
                {
                    if (!names.Contains(of[j]))
                    {
                        names.Add(of[j]);
                    }
                }
            }

            names.Sort(string.CompareOrdinal);

            return names.ToArray();
        }
    }
}
