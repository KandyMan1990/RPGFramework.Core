using RPGFramework.Audio.Music;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The music paused as a module was replaced — by a battle, say — for the module it returns to to resume. Empty
    /// when nothing was paused, as when a battle keeps the music playing.
    /// </summary>
    public interface IPausedMusicStore
    {
        /// <summary>Keeps the snapshot of the music paused, for the module returned to to take.</summary>
        void Set(MusicSnapshot snapshot);

        /// <summary>Hands back the paused music and empties the store; false when it holds none.</summary>
        bool TryTake(out MusicSnapshot snapshot);
    }

    internal sealed class PausedMusicStore : IPausedMusicStore
    {
        private MusicSnapshot m_Snapshot;
        private bool          m_Holds;

        void IPausedMusicStore.Set(MusicSnapshot snapshot)
        {
            m_Snapshot = snapshot;
            m_Holds    = true;
        }

        bool IPausedMusicStore.TryTake(out MusicSnapshot snapshot)
        {
            bool held = m_Holds;

            snapshot   = m_Snapshot;
            m_Snapshot = default;
            m_Holds    = false;

            return held;
        }
    }
}
