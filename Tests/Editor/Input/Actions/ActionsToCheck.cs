using System;
using System.Collections.Generic;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input.Actions
{
    /// <summary>
    /// The project-wide actions as a game sets them up — a <c>Player</c> map to move and confirm in, and a <c>UI</c>
    /// map — checked as a module scene's adapter routing <c>Player</c> is.
    /// </summary>
    internal sealed class ActionsToCheck
    {
        private readonly InputActionAsset m_Actions;

        internal ActionsToCheck()
        {
            m_Actions = ScriptableObject.CreateInstance<InputActionAsset>();
            m_Actions.name = "Game Actions";

            InputActionMap player = m_Actions.AddActionMap("Player");
            InputAction    move   = player.AddAction("Movement", InputActionType.Value, expectedControlLayout: "Vector2");

            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            player.AddAction("Confirm", InputActionType.Button, "<Keyboard>/e");

            m_Actions.AddActionMap("UI").AddAction("Submit", InputActionType.Button, "<Keyboard>/e");
        }

        internal InputActionMap Player => m_Actions.FindActionMap("Player");

        internal List<string> Check(string movement, params IRequiredActions[] modules)
        {
            return CheckAgainst(m_Actions, movement, modules);
        }

        internal List<string> CheckAgainst(InputActionAsset projectWide, string movement, params IRequiredActions[] modules)
        {
            List<string> problems = new List<string>();

            InputActionsCheck.Check(m_Actions, "Player", movement, projectWide, modules, "Field / 'Input'", problems);

            return problems;
        }

        internal void Destroy()
        {
            UnityEngine.Object.DestroyImmediate(m_Actions);
        }
    }

    /// <summary>A module reading the actions it is given.</summary>
    internal sealed class ModuleReading : IRequiredActions
    {
        private readonly string           m_Map;
        private readonly bool             m_Moves;
        private readonly RequiredAction[] m_Actions;

        internal ModuleReading(string map, bool moves, params RequiredAction[] actions)
        {
            m_Map     = map;
            m_Moves   = moves;
            m_Actions = actions;
        }

        internal static RequiredAction Button(string name)
        {
            return new RequiredAction(name, RequiredActionKind.Button, "what it does");
        }

        Type IRequiredActions.Module => typeof(ModuleReading);

        string IRequiredActions.Map => m_Map;

        bool IRequiredActions.Moves => m_Moves;

        IReadOnlyList<RequiredAction> IRequiredActions.Actions => m_Actions;
    }
}
