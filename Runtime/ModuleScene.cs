using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGFramework.Core
{
    internal sealed class ModuleScene : IModuleScene
    {
        private readonly Scene m_Scene;

        internal ModuleScene(Scene scene)
        {
            m_Scene = scene;
        }

        T IModuleScene.Find<T>()
        {
            // A copy: Scene is a mutable struct, so a readonly field's would be copied anyway, out of sight.
            Scene        scene = m_Scene;
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                T found = roots[i].GetComponentInChildren<T>();

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}