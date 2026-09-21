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

        private static readonly RequiredVariable[] s_Variables =
        {
            new RequiredVariable(CURRENT_MODULE, MemoryBank.Persistent, VariableWidth.Byte, "The module the playthrough is in. Its default is the module a new game begins in")
        };

        public IReadOnlyList<RequiredVariable> Variables => s_Variables;
    }
}
