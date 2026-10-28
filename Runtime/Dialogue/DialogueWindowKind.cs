namespace RPGFramework.Core.Dialogue
{
    /// <summary>How a dialogue window takes the player's press.</summary>
    public enum DialogueWindowKind
    {
        /// <summary>Waits for the player: keeps the press, and closes on one once it has finished.</summary>
        Message,

        /// <summary>Waits for an answer: keeps the press, and holds the focus until it is answered.</summary>
        Question,

        /// <summary>
        /// Left up while the player carries on: lets the press through to what is under it, and goes when a script closes
        /// it, or on a press once it has finished and a script waits for it.
        /// </summary>
        Background
    }
}
