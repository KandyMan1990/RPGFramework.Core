using UnityEngine.InputSystem;

namespace RPGFramework.Core.Input
{
    /// <summary>The actions the player's bindings are made on.</summary>
    internal interface IInputActionsProvider
    {
        /// <summary>Null when the project has none.</summary>
        InputActionAsset Actions { get; }
    }

    /// <summary>The project-wide actions, which UI Toolkit and every input adapter read.</summary>
    internal sealed class ProjectInputActionsProvider : IInputActionsProvider
    {
        InputActionAsset IInputActionsProvider.Actions => InputSystem.actions;
    }
}
