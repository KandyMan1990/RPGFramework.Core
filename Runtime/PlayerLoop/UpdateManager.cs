using System.Collections.Generic;

namespace RPGFramework.Core.PlayerLoop
{
    public static class UpdateManager
    {
        private static readonly List<IUpdatable>  m_Updatables = new List<IUpdatable>();
        private static readonly Queue<IUpdatable> m_Queue      = new Queue<IUpdatable>();

        public static void RegisterUpdatable(IUpdatable           player) => m_Updatables.Add(player);
        public static void UnregisterUpdatable(IUpdatable         player) => m_Updatables.Remove(player);
        public static void QueueForUnregisterUpdatable(IUpdatable player) => m_Queue.Enqueue(player);

        public static void UpdateListeners()
        {
            foreach (IUpdatable player in m_Updatables)
            {
                player.Update();
            }

            while (m_Queue.Count > 0)
            {
                UnregisterUpdatable(m_Queue.Dequeue());
            }
        }

        public static void ClearListeners() => m_Updatables.Clear();
    }
}