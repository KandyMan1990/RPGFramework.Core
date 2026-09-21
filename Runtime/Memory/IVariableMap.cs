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

        bool TryGetVariable(string varName, out VariableDefinition definition);
    }
}
