using System.Collections.Generic;
using RPGFramework.Core.Memory;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// Refuses to build a player from a project whose variable map is wrong — a required variable missing, or
    /// declared at the wrong width — because the framework reads those by name, and at run time there is no
    /// good answer to a map that lacks one.
    /// </summary>
    internal sealed class VariableMapBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            List<string> problems = new List<string>();

            string[] guids = AssetDatabase.FindAssets("t:" + nameof(VariableMapAsset));

            if (guids.Length == 0)
            {
                throw new BuildFailedException($"No {nameof(VariableMapAsset)} in the project. The framework reads the module to start in, and where in it, from the variable map — create one with RPG Framework / Core / Variable Map");
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];

                string           path = AssetDatabase.GUIDToAssetPath(guid);
                VariableMapAsset map  = AssetDatabase.LoadAssetAtPath<VariableMapAsset>(path);

                List<string> mapProblems = map.Validate();

                for (int j = 0; j < mapProblems.Count; j++)
                {
                    string problem = mapProblems[j];

                    problems.Add($"{path}: {problem}");
                }
            }

            if (problems.Count > 0)
            {
                throw new BuildFailedException($"The variable map has {problems.Count} problem(s):\n  {string.Join("\n  ", problems)}");
            }
        }
    }
}
