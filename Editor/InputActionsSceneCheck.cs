using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// Checks each module scene's input adapter as Unity processes the scene: a player build fails on a map missing an
    /// action its module reads, and a scene loaded in play mode logs it, so it is seen the first time the module runs.
    /// </summary>
    internal sealed class InputActionsSceneCheck : IProcessSceneWithReport
    {
        int IOrderedCallback.callbackOrder => 0;

        void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
        {
            List<string> problems = new List<string>();

            InputActionsCheck.CheckScene(scene, problems);

            if (problems.Count == 0)
            {
                return;
            }

            string message = $"{scene.name}'s input has {problems.Count} problem(s):\n  {string.Join("\n  ", problems)}";

            if (report == null)
            {
                Debug.LogError(message);
                return;
            }

            throw new BuildFailedException(message);
        }
    }
}
