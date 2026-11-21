namespace RPGFramework.Core.Databases
{
    /// <summary>
    /// What each of the game's characters is, by its name's hash, which a party's record holds in
    /// <see cref="Memory.CoreVariables.ROSTER_CHARACTER" />. Bound by the game, from a <see cref="CharacterDatabase" />.
    /// What else a game or a module knows of a character is a database of its own, keyed by the same hash.
    /// </summary>
    public interface ICharacterDatabase
    {
        /// <summary>The character this name's hash names; false for one the game does not have.</summary>
        bool TryGetDefinition(ulong character, out CharacterDefinition definition);
    }
}
