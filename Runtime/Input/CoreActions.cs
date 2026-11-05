using RPGFramework.Hashing;

namespace RPGFramework.Core.Input
{
    /// <summary>
    /// The actions Core reads, each the hash of the name a game gives it in its input actions, so an action is the same
    /// whichever map it is in. A module reading its own names them the same way, and declares what it reads as an
    /// <see cref="IRequiredActions" />, so a map lacking one is found before the game runs.
    /// </summary>
    public static class CoreActions
    {
        /// <summary>The name of the action that talks, answers and turns a page.</summary>
        public const string CONFIRM = "Confirm";

        /// <summary>The name of the action that backs out, of a question or a menu.</summary>
        public const string CANCEL = "Cancel";

        public static readonly ulong Confirm = Fnv1a64.Hash(CONFIRM);

        public static readonly ulong Cancel = Fnv1a64.Hash(CANCEL);
    }
}
