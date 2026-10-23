using System;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGFramework.Core
{
    public static class TaskExtensions
    {
        /// <summary>Lets the task run on without being awaited, logging an exception it ends with.</summary>
        public static void FireAndForget(this Task task)
        {
            _ = LogFailureAsync(task);
        }

        private static async Task LogFailureAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
