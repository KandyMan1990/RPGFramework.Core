using System.Collections.Generic;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// The game's variables as the running game sees them: where each lives and what a new game starts it at.
    /// Bound by the game, from its <see cref="VariableMapAsset" />.
    /// </summary>
    public interface IVariableMap
    {
        /// <summary>Every variable the game declares, in both banks, as authored.</summary>
        IReadOnlyList<VariableDefinition> Variables { get; }

        /// <summary>
        /// The id most recently given to a variable. Ids only go up, so a save holding a higher one was written by a
        /// newer build of the game.
        /// </summary>
        uint LastVariableId { get; }

        /// <summary>The variable authored under this name; false when there is none.</summary>
        bool TryGetVariable(string varName, out VariableDefinition definition);
    }
}
