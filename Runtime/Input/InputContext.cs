using UnityEngine;

namespace RPGFramework.Core.Input
{
    public enum ControlSlot
    {
        Primary,
        Secondary,
        Tertiary,
        Quaternary,
        ShoulderLeft,
        ShoulderRight,
        TriggerLeft,
        TriggerRight,
        Start,
        Select
    }

    /// <summary>
    /// Something that takes input while it is on the <see cref="IInputRouter" />'s stack: a menu, the field, a dialogue
    /// window. The router offers each press to the top context first, and on down until one takes it.
    /// </summary>
    public interface IInputContext
    {
        /// <summary>A button was pressed; true if this context took it, so none under it sees it.</summary>
        bool Handle(ControlSlot slot);

        /// <summary>The move input changed; true if this context took it, so none under it sees it.</summary>
        bool HandleMove(Vector2 move);
    }
}