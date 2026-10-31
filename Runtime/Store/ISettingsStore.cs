using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Hashing;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The player's settings Core's menus and modules read — the language, the volumes and the message speeds — as the
    /// settings file last held them or the player has set them since. <see cref="Settings.ISettingsService.Commit" />
    /// writes them.
    /// </summary>
    public interface ISettingsStore
    {
        /// <summary>Whether the player has chosen a language; false on a first launch, until they do.</summary>
        bool IsLanguageChosen { get; }

        /// <summary>Whether <paramref name="language" /> is the one the player chose.</summary>
        bool IsLanguage(string language);

        /// <summary>Records the language the player chose.</summary>
        void SetLanguage(string language);

        /// <summary>
        /// The player's music volume, 0 to 1, under which every song plays at its own, <see cref="ISongVolumeStore" />.
        /// </summary>
        float MusicVolume { get; }

        /// <summary>Records the player's music volume, 0 to 1. It does not change the music player's.</summary>
        void SetMusicVolume(float volume);

        /// <summary>The player's sound effects' volume, 0 to 1.</summary>
        float SfxVolume { get; }

        /// <summary>Records the player's sound effects' volume, 0 to 1. It does not change the sound effects player's.</summary>
        void SetSfxVolume(float volume);

        /// <summary>How fast battle messages appear, 0 to 1, 0.5 as authored.</summary>
        float BattleMessageSpeed { get; }

        /// <summary>Records how fast battle messages appear, 0 to 1.</summary>
        void SetBattleMessageSpeed(float speed);

        /// <summary>How fast the field's dialogue appears, 0 to 1, 0.5 as authored.</summary>
        float FieldMessageSpeed { get; }

        /// <summary>Records how fast the field's dialogue appears, 0 to 1.</summary>
        void SetFieldMessageSpeed(float speed);
    }

    /// <summary>
    /// Kept in Core's settings variables. The language is its name's hash, zero until one is chosen, so it fits a
    /// variable whatever its length.
    /// </summary>
    internal sealed class SettingsStore : ISettingsStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_LanguageAddress;
        private readonly ushort         m_MusicVolumeAddress;
        private readonly ushort         m_SfxVolumeAddress;
        private readonly ushort         m_BattleMessageSpeedAddress;
        private readonly ushort         m_FieldMessageSpeedAddress;

        public SettingsStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            m_MemoryService             = memoryService;
            m_LanguageAddress           = GetAddress(variableMap, CoreVariables.LANGUAGE);
            m_MusicVolumeAddress        = GetAddress(variableMap, CoreVariables.MUSIC_VOLUME);
            m_SfxVolumeAddress          = GetAddress(variableMap, CoreVariables.SFX_VOLUME);
            m_BattleMessageSpeedAddress = GetAddress(variableMap, CoreVariables.BATTLE_MESSAGE_SPEED);
            m_FieldMessageSpeedAddress  = GetAddress(variableMap, CoreVariables.FIELD_MESSAGE_SPEED);
        }

        bool ISettingsStore.IsLanguageChosen => m_MemoryService.ReadUlong(MemoryBank.Settings, m_LanguageAddress) != 0;

        bool ISettingsStore.IsLanguage(string language) => m_MemoryService.ReadUlong(MemoryBank.Settings, m_LanguageAddress) == Fnv1a64.Hash(language);

        void ISettingsStore.SetLanguage(string language) => m_MemoryService.WriteUlong(MemoryBank.Settings, m_LanguageAddress, Fnv1a64.Hash(language));

        float ISettingsStore.MusicVolume => m_MemoryService.ReadFloat(MemoryBank.Settings, m_MusicVolumeAddress);

        void ISettingsStore.SetMusicVolume(float volume) => m_MemoryService.WriteFloat(MemoryBank.Settings, m_MusicVolumeAddress, volume);

        float ISettingsStore.SfxVolume => m_MemoryService.ReadFloat(MemoryBank.Settings, m_SfxVolumeAddress);

        void ISettingsStore.SetSfxVolume(float volume) => m_MemoryService.WriteFloat(MemoryBank.Settings, m_SfxVolumeAddress, volume);

        float ISettingsStore.BattleMessageSpeed => m_MemoryService.ReadFloat(MemoryBank.Settings, m_BattleMessageSpeedAddress);

        void ISettingsStore.SetBattleMessageSpeed(float speed) => m_MemoryService.WriteFloat(MemoryBank.Settings, m_BattleMessageSpeedAddress, speed);

        float ISettingsStore.FieldMessageSpeed => m_MemoryService.ReadFloat(MemoryBank.Settings, m_FieldMessageSpeedAddress);

        void ISettingsStore.SetFieldMessageSpeed(float speed) => m_MemoryService.WriteFloat(MemoryBank.Settings, m_FieldMessageSpeedAddress, speed);

        private static ushort GetAddress(IVariableMap variableMap, string name)
        {
            variableMap.TryGetVariable(name, out VariableDefinition variable);

            return (ushort)variable.Offset;
        }
    }
}
