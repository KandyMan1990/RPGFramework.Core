namespace RPGFramework.Core.Audio
{
    /// <summary>
    /// The sounds every module shares — the cursor moving, a choice taken, backed out of or refused — and starting,
    /// loading and saving a game, each the name hash of a sound in the game's sound effect providers, <c>0</c> for none.
    /// The logic that decides plays them through the game's <c>ISfxPlayer</c>, never a UI. Bound by the game.
    /// </summary>
    public interface ISystemSoundConfig
    {
        /// <summary>The cursor moving to another choice.</summary>
        ulong Cursor { get; }

        /// <summary>A choice taken.</summary>
        ulong Confirm { get; }

        /// <summary>Backing out, or a question's cursor moved to its cancel answer.</summary>
        ulong Cancel { get; }

        /// <summary>A choice refused: nothing to load, saving not allowed here, a binding refused.</summary>
        ulong Error { get; }

        /// <summary>A new game begun.</summary>
        ulong NewGame { get; }

        /// <summary>A saved game chosen to load.</summary>
        ulong LoadGame { get; }

        /// <summary>The game saved.</summary>
        ulong SaveGame { get; }
    }
}
