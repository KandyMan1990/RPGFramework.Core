namespace RPGFramework.Core.Dialogue
{
    /// <summary>
    /// How a dialogue window looks, so spoken lines, thoughts and captions are told apart. Each is a USS class on
    /// the window — <c>dialogue-window--spoken</c> and so on — so a game restyles them in its own stylesheet.
    /// The numbers are bytecode and must not change.
    /// </summary>
    public enum DialogueWindowStyle : byte
    {
        /// <summary>
        /// A character speaking.
        /// </summary>
        Spoken = 0,

        /// <summary>
        /// A character's thoughts.
        /// </summary>
        Thought = 1,

        /// <summary>
        /// Text over the scene with no window behind it.
        /// </summary>
        Transparent = 2
    }
}