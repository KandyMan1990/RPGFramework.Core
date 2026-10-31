using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The volume the playthrough's songs play at, 0 to 1, as the scripts last set it, so it carries across modules
    /// and saves: a module plays its music at it, and a song paused for a battle fades back in to it. It is under the
    /// player's own music volume, <see cref="ISettingsStore.MusicVolume" />.
    /// </summary>
    public interface ISongVolumeStore
    {
        /// <summary>The song volume, 0 to 1.</summary>
        float SongVolume { get; }

        /// <summary>
        /// Records the song volume, 0 to 1, until it is set again. It does not change the song playing.
        /// </summary>
        void SetSongVolume(float volume);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.SONG_VOLUME" />, so a save holds it and a new game starts from its default.
    /// </summary>
    internal sealed class SongVolumeStore : ISongVolumeStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_Address;

        public SongVolumeStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.SONG_VOLUME, out VariableDefinition songVolume);

            m_MemoryService = memoryService;
            m_Address       = (ushort)songVolume.Offset;
        }

        float ISongVolumeStore.SongVolume
        {
            get
            {
                float volume = m_MemoryService.ReadFloat(MemoryBank.Persistent, m_Address);

                return volume;
            }
        }

        void ISongVolumeStore.SetSongVolume(float volume) => m_MemoryService.WriteFloat(MemoryBank.Persistent, m_Address, volume);
    }
}
