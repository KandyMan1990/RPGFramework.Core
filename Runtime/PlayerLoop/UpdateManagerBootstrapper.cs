using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace RPGFramework.Core.PlayerLoop
{
    /// <summary>
    /// Puts <see cref="UpdateManager" /> first in Unity's frame update and physics step as play begins — in the editor
    /// and as a player starts — and takes it out again as play ends.
    /// </summary>
    internal static partial class UpdateManagerBootstrapper
    {
        [OnEnteringPlayMode]
        private static void OnEnteringPlayMode()
        {
            PlayerLoopSystem loop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            PlayerLoopSystem update      = new PlayerLoopSystem { type = typeof(UpdateManager), updateDelegate = UpdateManager.UpdateListeners };
            PlayerLoopSystem fixedUpdate = new PlayerLoopSystem { type = typeof(UpdateManager), updateDelegate = UpdateManager.FixedUpdateListeners };

            if (!TryInsertFirst(ref loop, typeof(Update), update) || !TryInsertFirst(ref loop, typeof(FixedUpdate), fixedUpdate))
            {
                Debug.LogWarning($"{nameof(UpdateManagerBootstrapper)} could not find Unity's update and physics phases, so nothing registered with {nameof(UpdateManager)} will tick");
                return;
            }

            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(loop);
        }

        [OnExitingPlayMode]
        private static void OnExitingPlayMode()
        {
            PlayerLoopSystem loop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            RemoveAll(ref loop, typeof(UpdateManager));

            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(loop);
            UpdateManager.ClearListeners();
        }

        // First among the phase's own systems, wherever the phase sits in the loop.
        private static bool TryInsertFirst(ref PlayerLoopSystem loop, Type phase, PlayerLoopSystem system)
        {
            if (loop.type == phase)
            {
                PlayerLoopSystem[] existing = loop.subSystemList ?? Array.Empty<PlayerLoopSystem>();
                PlayerLoopSystem[] inserted = new PlayerLoopSystem[existing.Length + 1];

                inserted[0] = system;
                Array.Copy(existing, 0, inserted, 1, existing.Length);
                loop.subSystemList = inserted;

                return true;
            }

            if (loop.subSystemList == null)
            {
                return false;
            }

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (TryInsertFirst(ref loop.subSystemList[i], phase, system))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveAll(ref PlayerLoopSystem loop, Type type)
        {
            if (loop.subSystemList == null)
            {
                return;
            }

            int count = 0;

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (loop.subSystemList[i].type != type)
                {
                    count++;
                }
            }

            PlayerLoopSystem[] kept = new PlayerLoopSystem[count];
            int                next = 0;

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                PlayerLoopSystem system = loop.subSystemList[i];

                if (system.type == type)
                {
                    continue;
                }

                RemoveAll(ref system, type);
                kept[next++] = system;
            }

            loop.subSystemList = kept;
        }
    }
}
