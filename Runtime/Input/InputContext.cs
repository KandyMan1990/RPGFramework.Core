namespace RPGFramework.Core.Input
{
    /// <summary>
    /// Something that takes input while it is on the <see cref="IInputRouter" />'s stack: a menu, the field, a dialogue
    /// window. The router offers each press to the top context first, and on down until one takes it; movement is read
    /// each tick by what moves, reaching it only when no context above takes movement.
    /// </summary>
    public interface IInputContext
    {
        /// <summary>
        /// A button action was pressed, named by the hash of its name; true if this context took it, so none under it sees
        /// it.
        /// </summary>
        bool Handle(ulong action);

        /// <summary>True if this context takes movement, so none under it moves while it is on the stack.</summary>
        bool TakesMovement { get; }
    }
}