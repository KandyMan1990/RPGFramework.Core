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

#if UNITY_EDITOR
        private static readonly RequiredVariable[] m_Variables =
        {
            new RequiredVariable(CURRENT_MODULE,   MemoryBank.Persistent, VariableWidth.Byte,  "The module the playthrough is in. Its default is the module a new game begins in"),
            new RequiredVariable(LOADED_FROM_SAVE, MemoryBank.Session,    VariableWidth.Bool,  "Set when a save is loaded, and cleared once the player is back where it was made. Its default is not used"),
            new RequiredVariable(SAVE_ENABLED,     MemoryBank.Session,    VariableWidth.Bool,  "Whether the player can save from the party menu. Its default is the game's policy: false to save only where a script allows it, true to save anywhere"),
            new RequiredVariable(PLAY_TIME,        MemoryBank.Persistent, VariableWidth.UInt,  "How long the playthrough has been played, in whole seconds. Counted by the framework"),
            new RequiredVariable(LOCATION_NAME,    MemoryBank.Persistent, VariableWidth.ULong, "The place the player is in, as the hash of its localisation key. Written by the module the player is in")
        };

        IReadOnlyList<RequiredVariable> IRequiredVariables.Variables => m_Variables;
#endif
    }
}