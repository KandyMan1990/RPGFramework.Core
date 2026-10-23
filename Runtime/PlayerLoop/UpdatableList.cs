using System.Collections.Generic;

namespace RPGFramework.Core.PlayerLoop
{
    /// <summary>
    /// What one loop ticks, changed only between ticks: adds and removes are kept in the order they were asked for and
    /// applied together before the next tick, so the last asked of each stands and nothing is ticked twice.
    /// </summary>
    internal sealed class UpdatableList<T> where T : class
    {
        private readonly List<T>      m_Items;
        private readonly List<Change> m_Changes;

        internal IReadOnlyList<T> Items => m_Items;

        internal UpdatableList()
        {
            m_Items   = new List<T>();
            m_Changes = new List<Change>();
        }

        internal void Add(T item)
        {
            m_Changes.Add(new Change(item, true));
        }

        internal void Remove(T item)
        {
            m_Changes.Add(new Change(item, false));
        }

        internal void Apply()
        {
            for (int i = 0; i < m_Changes.Count; i++)
            {
                Change change = m_Changes[i];

                if (!change.Adds)
                {
                    m_Items.Remove(change.Item);
                }
                else if (!m_Items.Contains(change.Item))
                {
                    m_Items.Add(change.Item);
                }
            }

            m_Changes.Clear();
        }

        internal void Clear()
        {
            m_Items.Clear();
            m_Changes.Clear();
        }

        private readonly struct Change
        {
            internal T    Item { get; }
            internal bool Adds { get; }

            internal Change(T item, bool adds)
            {
                Item = item;
                Adds = adds;
            }
        }
    }
}
