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

        private static readonly List<IFixedUpdatable>  m_FixedUpdatables      = new List<IFixedUpdatable>();
        private static readonly Queue<IFixedUpdatable> m_FixedRegisterQueue   = new Queue<IFixedUpdatable>();
        private static readonly Queue<IFixedUpdatable> m_FixedUnregisterQueue = new Queue<IFixedUpdatable>();

        public static void RegisterUpdatable(IUpdatable   player) => m_RegisterQueue.Enqueue(player);
        public static void UnregisterUpdatable(IUpdatable player) => m_UnregisterQueue.Enqueue(player);

        public static void RegisterFixedUpdatable(IFixedUpdatable   player) => m_FixedRegisterQueue.Enqueue(player);
        public static void UnregisterFixedUpdatable(IFixedUpdatable player) => m_FixedUnregisterQueue.Enqueue(player);

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

        internal static void FixedUpdateListeners()
        {
            while (m_FixedUnregisterQueue.Count > 0)
            {
                m_FixedUpdatables.Remove(m_FixedUnregisterQueue.Dequeue());
            }

            while (m_FixedRegisterQueue.Count > 0)
            {
                m_FixedUpdatables.Add(m_FixedRegisterQueue.Dequeue());
            }

            for (int i = 0; i < m_FixedUpdatables.Count; i++)
            {
                m_FixedUpdatables[i].FixedUpdate();
            }
        }

        internal static void ClearListeners()
        {
            m_Updatables.Clear();
            m_RegisterQueue.Clear();
            m_UnregisterQueue.Clear();

            m_FixedUpdatables.Clear();
            m_FixedRegisterQueue.Clear();
            m_FixedUnregisterQueue.Clear();
        }
    }
}