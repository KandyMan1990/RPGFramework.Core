namespace RPGFramework.Core.Input
{
    /// <summary>
    /// One control the player can rebind: the actions it is in, each <c>Map/Action</c> — a confirm in the gameplay map and
    /// UI Toolkit's <c>UI/Submit</c> rebound as one — and the part of a composite it is, such as a direction's <c>up</c>,
    /// or none.
    /// </summary>
    public readonly struct RebindableControl
    {
        internal readonly string[] Actions;
        internal readonly string   Part;

        public RebindableControl(string[] actions, string part)
        {
            Actions = actions;
            Part    = part;
        }
    }
}
