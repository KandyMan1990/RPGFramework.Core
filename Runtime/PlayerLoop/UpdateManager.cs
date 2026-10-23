using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;

namespace RPGFramework.Core.PlayerLoop
{
    /// <summary>
    /// Ticks what registers, once a frame and once a physics step, from the player loop. A change asked for during a
    /// tick takes effect before the next, so a module that unregisters as it suspends gets no further tick.
    /// </summary>
    [AutoStaticsCleanup]
    public static partial class UpdateManager
    {
        private static readonly UpdatableList<IUpdatable>      m_Updatables      = new UpdatableList<IUpdatable>();
        private static readonly UpdatableList<IFixedUpdatable> m_FixedUpdatables = new UpdatableList<IFixedUpdatable>();

        public static void RegisterUpdatable(IUpdatable updatable) => m_Updatables.Add(updatable);

        public static void UnregisterUpdatable(IUpdatable updatable) => m_Updatables.Remove(updatable);

        public static void RegisterFixedUpdatable(IFixedUpdatable updatable) => m_FixedUpdatables.Add(updatable);

        public static void UnregisterFixedUpdatable(IFixedUpdatable updatable) => m_FixedUpdatables.Remove(updatable);

        internal static void UpdateListeners()
        {
            m_Updatables.Apply();

            IReadOnlyList<IUpdatable> updatables = m_Updatables.Items;

            for (int i = 0; i < updatables.Count; i++)
            {
                updatables[i].Update();
            }
        }

        internal static void FixedUpdateListeners()
        {
            m_FixedUpdatables.Apply();

            IReadOnlyList<IFixedUpdatable> updatables = m_FixedUpdatables.Items;

            for (int i = 0; i < updatables.Count; i++)
            {
                updatables[i].FixedUpdate();
            }
        }

        internal static void ClearListeners()
        {
            m_Updatables.Clear();
            m_FixedUpdatables.Clear();
        }
    }
}
