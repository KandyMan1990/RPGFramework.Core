using System;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace RPGFramework.Core.PlayerLoop
{
    internal static class PlayerLoopUtils
    {
        public static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
        {
            if (loop.type != typeof(T))
            {
                return HandleSubSystemLoop<T>(ref loop, systemToInsert, index);
            }

            PlayerLoopSystem[] existing = loop.subSystemList ?? Array.Empty<PlayerLoopSystem>();
            PlayerLoopSystem[] inserted = new PlayerLoopSystem[existing.Length + 1];

            Array.Copy(existing, 0, inserted, 0, index);
            inserted[index] = systemToInsert;
            Array.Copy(existing, index, inserted, index + 1, existing.Length - index);

            loop.subSystemList = inserted;

            return true;
        }

        public static void RemoveSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null)
                return;

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (loop.subSystemList[i].type == systemToRemove.type && loop.subSystemList[i].updateDelegate == systemToRemove.updateDelegate)
                {
                    PlayerLoopSystem[] existing = loop.subSystemList;
                    PlayerLoopSystem[] removed  = new PlayerLoopSystem[existing.Length - 1];

                    Array.Copy(existing, 0, removed, 0, i);
                    Array.Copy(existing, i + 1, removed, i, existing.Length - i - 1);

                    loop.subSystemList = removed;
                }
            }

            HandleSubSystemForRemoval<T>(ref loop, systemToRemove);
        }

        private static void HandleSubSystemForRemoval<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null)
                return;

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                RemoveSystem<T>(ref loop.subSystemList[i], systemToRemove);
            }
        }

        private static bool HandleSubSystemLoop<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
        {
            if (loop.subSystemList == null)
                return false;

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (!InsertSystem<T>(ref loop.subSystemList[i], in systemToInsert, index))
                    continue;

                return true;
            }

            return false;
        }

        public static void PrintPlayerLoop(PlayerLoopSystem loop)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Unity Player Loop");

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                PlayerLoopSystem loopSystem = loop.subSystemList[i];

                PrintSubSystem(loopSystem, sb, 0);
            }

            Debug.Log(sb.ToString());
        }

        private static void PrintSubSystem(PlayerLoopSystem loop, StringBuilder sb, int level)
        {
            sb.Append(' ', level * 2).AppendLine(loop.type.ToString());

            if (loop.subSystemList == null || loop.subSystemList.Length == 0)
                return;

            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                PlayerLoopSystem subSystem = loop.subSystemList[i];

                PrintSubSystem(subSystem, sb, level + 1);
            }
        }
    }
}