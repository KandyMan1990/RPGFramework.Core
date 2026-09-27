using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// The variables Core itself requires of every game's map.
    /// </summary>
    public sealed class CoreVariables : IRequiredVariables
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

        private static readonly RequiredVariable[] s_Variables =
        {
            new RequiredVariable(CURRENT_MODULE,   MemoryBank.Persistent, VariableWidth.Byte, "The module the playthrough is in. Its default is the module a new game begins in"),
            new RequiredVariable(LOADED_FROM_SAVE, MemoryBank.Session,    VariableWidth.Bool, "Set when a save is loaded, and cleared once the player is back where it was made. Its default is not used")
        };

        public IReadOnlyList<RequiredVariable> Variables => s_Variables;
    }
}