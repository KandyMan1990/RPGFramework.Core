using System.Collections.Generic;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// The game's variables as the running game sees them: where each lives and what a new game starts it at.
    /// Bound by the game, from its <see cref="VariableMapAsset" />.
    /// </summary>
    public interface IVariableMap
    {
        IReadOnlyList<VariableDefinition> Variables { get; }

        /// <summary>
        /// The id most recently given to a variable. Ids only go up, so a save holding a higher one was written by a
        /// newer build of the game.
        /// </summary>
        uint LastVariableId { get; }

        bool TryGetVariable(string varName, out VariableDefinition definition);
    }
}
