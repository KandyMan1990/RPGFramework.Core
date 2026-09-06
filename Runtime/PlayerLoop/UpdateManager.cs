using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;

namespace RPGFramework.Core.PlayerLoop
{
    [AutoStaticsCleanup]
    public static partial class UpdateManager
    {
        private static readonly List<IUpdatable>  m_Updatables      = new List<IUpdatable>();
        private static readonly Queue<IUpdatable> m_RegisterQueue   = new Queue<IUpdatable>();
        private static readonly Queue<IUpdatable> m_UnregisterQueue = new Queue<IUpdatable>();

        public static void RegisterUpdatable(IUpdatable   player) => m_RegisterQueue.Enqueue(player);
        public static void UnregisterUpdatable(IUpdatable player) => m_UnregisterQueue.Enqueue(player);

        internal static void UpdateListeners()
        {
            for (int i = 0; i < m_Updatables.Count; i++)
            {
                m_Updatables[i].Update();
            }

            while (m_UnregisterQueue.Count > 0)
            {
                m_Updatables.Remove(m_UnregisterQueue.Dequeue());
            }

            while (m_RegisterQueue.Count > 0)
            {
                m_Updatables.Add(m_RegisterQueue.Dequeue());
            }
        }

        internal static void ClearListeners()
        {
            m_Updatables.Clear();
            m_RegisterQueue.Clear();
            m_UnregisterQueue.Clear();
        }
    }
}