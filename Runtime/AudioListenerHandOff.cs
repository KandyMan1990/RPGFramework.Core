using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGFramework.Core
{
    /// <summary>
    /// Keeps exactly one audio listener on while module scenes stack. A scene loaded over others takes the listener
    /// over as it appears, if it brings one of its own, and hands it back before it goes. Each switch happens in the
    /// frame the scene appears or before it unloads, so there is never a frame with no listener, or with two.
    /// </summary>
    internal sealed class AudioListenerHandOff
    {
        // The listeners each scene loaded over others switched off, by that scene.
        private readonly Dictionary<Scene, AudioListener[]> m_Covered;

        private string m_Loading;

        internal AudioListenerHandOff()
        {
            m_Covered = new Dictionary<Scene, AudioListener[]>();
        }

        internal async Task LoadOverAsync(string sceneName)
        {
            m_Loading = sceneName;

            // Raised as the scene's objects are enabled, in the same frame, before Unity next looks for a listener.
            SceneManager.sceneLoaded += OnSceneLoaded;

            try
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            finally
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                m_Loading = null;
            }
        }

        /// <summary>
        /// Before a scene loaded over others unloads: its listeners off and those it covered back on, in one step.
        /// </summary>
        internal void HandBack(Scene scene)
        {
            if (!m_Covered.Remove(scene, out AudioListener[] covered))
            {
                return;
            }

            AudioListener[] own = ListenersIn(scene);

            for (int i = 0; i < own.Length; i++)
            {
                own[i].enabled = false;
            }

            for (int i = 0; i < covered.Length; i++)
            {
                if (covered[i] != null)
                {
                    covered[i].enabled = true;
                }
            }
        }

        /// <summary>A scene loaded alone replaces every other, along with what they covered.</summary>
        internal void ForgetAll()
        {
            m_Covered.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != m_Loading)
            {
                return;
            }

            AudioListener[] own = ListenersIn(scene);

            if (own.Length == 0)
            {
                return;
            }

            AudioListener[] all   = Object.FindObjectsByType<AudioListener>();
            int             count = 0;

            for (int i = 0; i < all.Length; i++)
            {
                if (Covers(all[i], scene))
                {
                    count++;
                }
            }

            AudioListener[] covered = new AudioListener[count];
            int             next    = 0;

            for (int i = 0; i < all.Length; i++)
            {
                AudioListener listener = all[i];

                if (!Covers(listener, scene))
                {
                    continue;
                }

                listener.enabled = false;
                covered[next++]  = listener;
            }

            m_Covered[scene] = covered;
        }

        private static bool Covers(AudioListener listener, Scene scene)
        {
            bool covers = listener.enabled && listener.gameObject.scene != scene;

            return covers;
        }

        private static AudioListener[] ListenersIn(Scene scene)
        {
            GameObject[]      roots = scene.GetRootGameObjects();
            AudioListener[][] found = new AudioListener[roots.Length][];
            int               count = 0;

            for (int i = 0; i < roots.Length; i++)
            {
                found[i] = roots[i].GetComponentsInChildren<AudioListener>();

                for (int j = 0; j < found[i].Length; j++)
                {
                    if (found[i][j].enabled)
                    {
                        count++;
                    }
                }
            }

            AudioListener[] enabled = new AudioListener[count];
            int             next    = 0;

            for (int i = 0; i < found.Length; i++)
            {
                for (int j = 0; j < found[i].Length; j++)
                {
                    if (found[i][j].enabled)
                    {
                        enabled[next++] = found[i][j];
                    }
                }
            }

            return enabled;
        }
    }
}
