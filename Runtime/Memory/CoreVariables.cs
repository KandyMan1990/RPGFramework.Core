using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// The variables Core itself requires of every game's map.
    /// </summary>
    public sealed class CoreVariables
#if UNITY_EDITOR
        : IRequiredVariables
#endif
    {
        /// <summary>
        /// The module the playthrough is in. Its default is where a new game begins; a module that is a place
        /// the player can be — not a menu, not a battle — writes itself here on entering, so a save resumes in
        /// it.
        /// </summary>
        public const string CURRENT_MODULE = "CurrentModule";

        /// <summary>
        /// Whether the playthrough came from a save file rather than a new game. Set when a save is loaded; the
        /// module the save resumes in uses it to put the player back where the save was made, then clears it, so
        /// nothing after that first arrival mistakes itself for a load.
        /// </summary>
        public const string LOADED_FROM_SAVE = "LoadedFromSave";

        /// <summary>
        /// Whether the player can save from the party menu right now. Its default is the game's policy: false to save
        /// only where a script allows it, such as at a save point, true to save anywhere a script has not forbidden.
        /// A module resets it to that default on entering a place.
        /// </summary>
        public const string SAVE_ENABLED = "SaveEnabled";

        /// <summary>
        /// How long the playthrough has been played, in whole seconds, counted by the framework in every module.
        /// </summary>
        public const string PLAY_TIME = "PlayTime";

        /// <summary>
        /// The place the player is in, as the hash of its localisation key, shown in the party menu and in each save's
        /// slot. Written by the module the player is in.
        /// </summary>
        public const string LOCATION_NAME = "LocationName";

        /// <summary>
        /// The track every battle plays, as its music asset's name hash, until a script sets another. Its default is a
        /// new game's; zero plays none.
        /// </summary>
        public const string BATTLE_MUSIC = "BattleMusic";

        /// <summary>
        /// The volume the playthrough's songs play at, 0 to 1, as the scripts last set it, kept across modules,
        /// battles and saves. A module's music plays at it, and a song a battle paused fades back in to it, under the
        /// player's own <see cref="MUSIC_VOLUME" />.
        /// </summary>
        public const string SONG_VOLUME = "SongVolume";

        /// <summary>
        /// The party's money, a whole number. Its default is what a new game starts with; what it is called is the
        /// game's text, so one game's gil is another's dollars.
        /// </summary>
        public const string MONEY = "Money";

        /// <summary>
        /// The language the player chose, as its name's hash; zero until they choose one, which a first launch asks for.
        /// </summary>
        public const string LANGUAGE = "Language";

        /// <summary>
        /// The player's music volume, 0 to 1, under which every song plays at the volume the scripts set,
        /// <see cref="SONG_VOLUME" />.
        /// </summary>
        public const string MUSIC_VOLUME = "MusicVolume";

        /// <summary>The player's sound effects' volume, 0 to 1.</summary>
        public const string SFX_VOLUME = "SfxVolume";

        /// <summary>How fast battle messages appear, 0 to 1, as the player set it: 0.5 is as authored.</summary>
        public const string BATTLE_MESSAGE_SPEED = "BattleMessageSpeed";

        /// <summary>How fast the field's dialogue appears, 0 to 1, as the player set it: 0.5 is as authored.</summary>
        public const string FIELD_MESSAGE_SPEED = "FieldMessageSpeed";

#if UNITY_EDITOR
        private static readonly RequiredVariable[] m_Variables =
        {
            new RequiredVariable(CURRENT_MODULE,       MemoryBank.Persistent, VariableWidth.Byte,  "The module the playthrough is in. Its default is the module a new game begins in"),
            new RequiredVariable(LOADED_FROM_SAVE,     MemoryBank.Session,    VariableWidth.Bool,  "Set when a save is loaded, and cleared once the player is back where it was made. Its default is not used"),
            new RequiredVariable(SAVE_ENABLED,         MemoryBank.Session,    VariableWidth.Bool,  "Whether the player can save from the party menu. Its default is the game's policy: false to save only where a script allows it, true to save anywhere"),
            new RequiredVariable(PLAY_TIME,            MemoryBank.Persistent, VariableWidth.UInt,  "How long the playthrough has been played, in whole seconds. Counted by the framework"),
            new RequiredVariable(LOCATION_NAME,        MemoryBank.Persistent, VariableWidth.ULong, "The place the player is in, as the hash of its localisation key. Written by the module the player is in"),
            new RequiredVariable(BATTLE_MUSIC,         MemoryBank.Persistent, VariableWidth.ULong, "The track every battle plays, as its music asset's name hash, until a script sets another. Its default is a new game's; zero plays none"),
            new RequiredVariable(SONG_VOLUME,          MemoryBank.Persistent, VariableWidth.Float, "The volume the songs play at, 0 to 1, as the scripts last set it, under the player's music volume. Its default is a new game's", VariableDefaults.FromFloat(1f)),
            new RequiredVariable(MONEY,                MemoryBank.Persistent, VariableWidth.UInt,  "The party's money. Its default is what a new game starts with"),
            new RequiredVariable(LANGUAGE,             MemoryBank.Settings,   VariableWidth.ULong, "The language the player chose, as its name's hash. Leave its default at zero, which asks the player on a first launch"),
            new RequiredVariable(MUSIC_VOLUME,         MemoryBank.Settings,   VariableWidth.Float, "The player's music volume, 0 to 1. Its default is a first launch's", VariableDefaults.FromFloat(1f)),
            new RequiredVariable(SFX_VOLUME,           MemoryBank.Settings,   VariableWidth.Float, "The player's sound effects' volume, 0 to 1. Its default is a first launch's", VariableDefaults.FromFloat(1f)),
            new RequiredVariable(BATTLE_MESSAGE_SPEED, MemoryBank.Settings,   VariableWidth.Float, "How fast battle messages appear, 0 to 1, 0.5 as authored. Its default is a first launch's", VariableDefaults.FromFloat(0.5f)),
            new RequiredVariable(FIELD_MESSAGE_SPEED,  MemoryBank.Settings,   VariableWidth.Float, "How fast the field's dialogue appears, 0 to 1, 0.5 as authored. Its default is a first launch's", VariableDefaults.FromFloat(0.5f))
        };

        IReadOnlyList<RequiredVariable> IRequiredVariables.Variables => m_Variables;
#endif
    }
}