using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPGFramework.Core.Input
{
    /// <summary>
    /// The player's own bindings, made through the Input System's rebinding on the project-wide actions and kept in the
    /// Settings bank, so they come back as the game starts. A control scheme is named as the actions name it,
    /// <c>Keyboard&amp;Mouse</c> or <c>Gamepad</c>.
    /// </summary>
    public interface IInputRebindingService
    {
        /// <summary>The control the player presses for <paramref name="control" /> in the scheme, as they read it; empty for none.</summary>
        string ControlName(RebindableControl control, string scheme);

        /// <summary>
        /// Waits for the player to press the control to bind, of the scheme's devices — any, Escape included; a few seconds
        /// of nothing cancels — and binds every action of <paramref name="control" /> to it. One of
        /// <paramref name="others" /> sharing a map with it, already bound to that control there, takes the control this
        /// one had, and one that displaces in turn the other, so each stays reachable; a binding that would still leave
        /// two of them on one control in a map is refused, changing nothing.
        /// </summary>
        Task<RebindOutcome> RebindAsync(RebindableControl control, string scheme, IReadOnlyList<RebindableControl> others);

        /// <summary>Puts every binding back as the game's actions have it.</summary>
        void ResetAll();
    }

    public enum RebindOutcome
    {
        Bound,
        Cancelled,

        /// <summary>The control pressed would have left two of the controls on one control in a map.</summary>
        Refused
    }

    /// <summary>Puts the player's stored bindings on the actions, once, as the game starts.</summary>
    internal interface IInputBindingsLoader
    {
        void Load();
    }
}
