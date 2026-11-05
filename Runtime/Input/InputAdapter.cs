using System;
using RPGFramework.DI;
using RPGFramework.Hashing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace RPGFramework.Core.Input
{
    /// <summary>
    /// A module's input: every button action in one map of the game's input actions, routed through the
    /// <see cref="IInputRouter" /> by the hash of its name, and the action the player moves with, read each tick. A
    /// game adds an action to its map and it is routed, with nothing to wire here.
    /// </summary>
    public sealed class InputAdapter : MonoBehaviour
#if ENABLE_RPG_FRAMEWORK_DEBUG
        , IDebugHeldControls
#endif
    {
        [Tooltip("The game's input actions; the project-wide actions, so UI Toolkit reads the same ones")]
        [SerializeField] private InputActionAsset m_Actions;

        [Tooltip("The map this module's input comes from: every button action in it is routed by name")]
        [SerializeField] private string m_Map;

        [Tooltip("The action in that map the player moves with; empty in a module where nothing moves")]
        [SerializeField] private string m_Movement;

        private IInputRouter m_InputRouter;
        private bool         m_Subscribed;

        // Found when input first starts and kept, so stopping it never looks an action up.
        private InputAction                           m_MovementAction;
        private InputAction[]                         m_Buttons;
        private ulong[]                               m_ButtonNames;
        private Action<InputAction.CallbackContext>[] m_Handlers;

        /// <summary>
        /// The direction held, read each tick by what moves; zero while input is off. Only where the router says movement
        /// reaches it does anything act on it.
        /// </summary>
        public Vector2 Movement => m_Subscribed && m_MovementAction != null ? m_MovementAction.ReadValue<Vector2>() : Vector2.zero;

        [Inject]
        private void Inject(IInputRouter inputRouter)
        {
            m_InputRouter = inputRouter;
        }

        private void OnDisable()
        {
            Disable();
        }

        public void Enable()
        {
            if (m_Subscribed)
            {
                return;
            }

            m_Subscribed = true;

            if (m_Buttons == null)
            {
                FindActions();
            }

            // Never turned off again: an action turned back on reads nothing held until it changes, so a direction held
            // through a transition would go unseen. Buttons are, so one held through a change does not act.
            m_MovementAction?.Enable();

            for (int i = 0; i < m_Buttons.Length; i++)
            {
                m_Buttons[i].performed += m_Handlers[i];
                m_Buttons[i].Enable();
            }
        }

        public void Disable()
        {
            if (!m_Subscribed)
            {
                return;
            }

            m_Subscribed = false;

            for (int i = 0; i < m_Buttons.Length; i++)
            {
                m_Buttons[i].Disable();
                m_Buttons[i].performed -= m_Handlers[i];
            }
        }

        private void FindActions()
        {
            InputActionMap             map     = m_Actions.FindActionMap(m_Map);
            ReadOnlyArray<InputAction> actions = map.actions;

            m_MovementAction = string.IsNullOrEmpty(m_Movement) ? null : map.FindAction(m_Movement);

            int count = 0;

            for (int i = 0; i < actions.Count; i++)
            {
                if (IsButton(actions[i]))
                {
                    count++;
                }
            }

            m_Buttons     = new InputAction[count];
            m_ButtonNames = new ulong[count];
            m_Handlers    = new Action<InputAction.CallbackContext>[count];

            int button = 0;

            for (int i = 0; i < actions.Count; i++)
            {
                InputAction action = actions[i];

                if (!IsButton(action))
                {
                    continue;
                }

                ulong name = Fnv1a64.Hash(action.name);

                m_Buttons[button]     = action;
                m_ButtonNames[button] = name;
                m_Handlers[button]    = _ => m_InputRouter.Route(name);

                button++;
            }
        }

        private bool IsButton(InputAction action)
        {
            return action.type == InputActionType.Button && action != m_MovementAction;
        }

#if ENABLE_RPG_FRAMEWORK_DEBUG
        // The actions are enabled only while input is on.
        bool IDebugHeldControls.IsHeld(ulong action)
        {
            if (!m_Subscribed)
            {
                return false;
            }

            int index = Array.IndexOf(m_ButtonNames, action);

            return index >= 0 && m_Buttons[index].IsPressed();
        }
#endif
    }
}
