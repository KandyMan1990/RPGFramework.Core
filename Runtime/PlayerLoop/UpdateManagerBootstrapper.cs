using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace RPGFramework.Core.PlayerLoop
{
    public static partial class UpdateManagerBootstrapper
    {
        private static PlayerLoopSystem m_UpdatePlayerLoopSystem;

        [OnEnteringPlayMode]
        private static void OnEnteringPlayMode()
        {
            PlayerLoopSystem currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            if (!InsertUpdateManagerPlayerLoop<Update>(ref currentPlayerLoop, 0))
            {
                Debug.LogWarning($"{nameof(UpdateManagerBootstrapper)} couldn't initialize {nameof(UpdateManager)} player loop system.");
                return;
            }

            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            //PlayerLoopUtils.PrintPlayerLoop(currentPlayerLoop);
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

        [OnExitingPlayMode]
        private static void OnExitingPlayMode()
        {
            PlayerLoopSystem currentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            RemoveUpdateManagerPlayerLoop<Update>(ref currentPlayerLoop);
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(currentPlayerLoop);

            UpdateManager.ClearListeners();
        }
    }
}