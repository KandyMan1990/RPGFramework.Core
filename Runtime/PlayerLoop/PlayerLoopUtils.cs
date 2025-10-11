using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace RPGFramework.Core.PlayerLoop
{
    public static class PlayerLoopUtils
    {
        public static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
        {
            if (loop.type != typeof(T))
            {
                return HandleSubSystemLoop<T>(ref loop, systemToInsert, index);
            }

            List<PlayerLoopSystem> playerLoopSystemList = new List<PlayerLoopSystem>();

            if (loop.subSystemList != null)
                playerLoopSystemList.AddRange(loop.subSystemList);

            playerLoopSystemList.Insert(index, systemToInsert);
            loop.subSystemList = playerLoopSystemList.ToArray();

            return true;
        }

        public static void RemoveSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null)
                return;

            List<PlayerLoopSystem> playerLoopSystemList = new List<PlayerLoopSystem>(loop.subSystemList);

            for (int i = 0; i < playerLoopSystemList.Count; i++)
            {
                if (playerLoopSystemList[i].type == systemToRemove.type && playerLoopSystemList[i].updateDelegate == systemToRemove.updateDelegate)
                {
                    playerLoopSystemList.RemoveAt(i);
                    loop.subSystemList = playerLoopSystemList.ToArray();
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

            foreach (PlayerLoopSystem loopSystem in loop.subSystemList)
            {
                PrintSubSystem(loopSystem, sb, 0);
            }

            Debug.Log(sb.ToString());
        }

        private static void PrintSubSystem(PlayerLoopSystem loop, StringBuilder sb, int level)
        {
            sb.Append(' ', level * 2).AppendLine(loop.type.ToString());

            if (loop.subSystemList == null || loop.subSystemList.Length == 0)
                return;

            foreach (PlayerLoopSystem subSystem in loop.subSystemList)
            {
                PrintSubSystem(subSystem, sb, level + 1);
            }
        }
    }
}