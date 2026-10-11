using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The volume the playthrough's music plays at, 0 to 1, as the scripts last set it, so it carries across modules
    /// and saves: a module plays its music at it, and a song paused for a battle fades back in to it.
    /// </summary>
    public interface IMusicVolumeStore
    {
        /// <summary>The music volume, 0 to 1.</summary>
        float MusicVolume { get; }

        /// <summary>
        /// Records the music volume, 0 to 1, until it is set again. It does not change the song playing.
        /// </summary>
        void SetMusicVolume(float volume);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.MUSIC_VOLUME" />, so a save holds it and a new game starts from its default.
    /// </summary>
    internal sealed class MusicVolumeStore : IMusicVolumeStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_Address;

        public MusicVolumeStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.MUSIC_VOLUME, out VariableDefinition musicVolume);

            m_MemoryService = memoryService;
            m_Address       = (ushort)musicVolume.Offset;
        }

        float IMusicVolumeStore.MusicVolume
        {
            get
            {
                float volume = m_MemoryService.ReadFloat(MemoryBank.Persistent, m_Address);

                return volume;
            }
        }

        void IMusicVolumeStore.SetMusicVolume(float volume) => m_MemoryService.WriteFloat(MemoryBank.Persistent, m_Address, volume);
    }
}
