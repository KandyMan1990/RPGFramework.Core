using System;

namespace RPGFramework.Core.Databases
{
    /// <summary>
    /// Which module each module id names, for Core to resolve as it enters one. Bound by the game, which knows its
    /// modules, its own included.
    /// </summary>
    public interface IModuleDatabase
    {
        /// <summary>
        /// The interface the module with this id is bound to in its scene's installer, such as <c>IFieldModule</c>.
        /// </summary>
        Type GetModuleType(byte moduleId);
    }
}