using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace RPGFramework.Core.PlayerLoop
{
    public static class UpdateManagerBootstrapper
    {
        private static PlayerLoopSystem m_UpdatePlayerLoopSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            PlayerLoopSystem currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            if (!InsertUpdateManagerPlayerLoop<Update>(ref currentPlayerLoop, 0))
            {
                Debug.LogWarning($"{nameof(UpdateManagerBootstrapper)} couldn't initialize {nameof(UpdateManager)} player loop system.");
                return;
            }

            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            //PlayerLoopUtils.PrintPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeStateChange)
            {
                if (playModeStateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    PlayerLoopSystem currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
                    RemoveUpdateManagerPlayerLoop<Update>(ref currentPlayerLoop);
                    UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                    UpdateManager.ClearListeners();
                }
            }
#endif
        }

        private static bool InsertUpdateManagerPlayerLoop<T>(ref PlayerLoopSystem loop, int index)
        {
            m_UpdatePlayerLoopSystem = new PlayerLoopSystem
                                       {
                                               type           = typeof(UpdateManager),
                                               updateDelegate = UpdateManager.UpdateListeners,
                                               subSystemList  = null
                                       };

            return PlayerLoopUtils.InsertSystem<T>(ref loop, in m_UpdatePlayerLoopSystem, index);
        }

        private static void RemoveUpdateManagerPlayerLoop<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopUtils.RemoveSystem<T>(ref loop, in m_UpdatePlayerLoopSystem);
        }
    }
}