using System;
using RPGFramework.DI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Input
{
    public sealed class InputAdapter : MonoBehaviour
    {
        private static readonly int m_SlotCount = Enum.GetValues(typeof(ControlSlot)).Length;

        [SerializeField] private InputActionReference m_Movement;
        [SerializeField] private InputActionReference m_Primary;
        [SerializeField] private InputActionReference m_Secondary;
        [SerializeField] private InputActionReference m_Tertiary;
        [SerializeField] private InputActionReference m_Quaternary;
        [SerializeField] private InputActionReference m_ShoulderLeft;
        [SerializeField] private InputActionReference m_ShoulderRight;
        [SerializeField] private InputActionReference m_TriggerLeft;
        [SerializeField] private InputActionReference m_TriggerRight;
        [SerializeField] private InputActionReference m_Start;
        [SerializeField] private InputActionReference m_Select;

        private IInputRouter m_InputRouter;
        private bool         m_Subscribed;

        // Read from the references when input starts and kept, so stopping it never reads a reference: one that no longer
        // resolves as play mode ends would throw.
        private InputAction                           m_MovementAction;
        private InputAction[]                         m_Actions;
        private Action<InputAction.CallbackContext>[] m_Handlers;

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

            ReadActions();

            if (m_MovementAction != null)
            {
                m_MovementAction.performed += RouteMovement;
                m_MovementAction.canceled  += RouteMovement;
                m_MovementAction.Enable();
            }

            for (int i = 0; i < m_Actions.Length; i++)
            {
                if (m_Actions[i] == null)
                {
                    continue;
                }

                m_Actions[i].performed += m_Handlers[i];
                m_Actions[i].Enable();
            }
        }

        public void Disable()
        {
            if (!m_Subscribed)
            {
                return;
            }

            m_Subscribed = false;

            if (m_MovementAction != null)
            {
                m_MovementAction.Disable();
                m_MovementAction.canceled  -= RouteMovement;
                m_MovementAction.performed -= RouteMovement;
            }

            for (int i = 0; i < m_Actions.Length; i++)
            {
                if (m_Actions[i] == null)
                {
                    continue;
                }

                m_Actions[i].Disable();
                m_Actions[i].performed -= m_Handlers[i];
            }
        }

        private void ReadActions()
        {
            if (m_Actions == null)
            {
                m_Actions  = new InputAction[m_SlotCount];
                m_Handlers = new Action<InputAction.CallbackContext>[m_SlotCount];

                for (int i = 0; i < m_SlotCount; i++)
                {
                    ControlSlot slot = (ControlSlot)i;

                    m_Handlers[i] = _ => m_InputRouter.Route(slot);
                }
            }

            m_MovementAction = ReadAction(m_Movement);

            m_Actions[(int)ControlSlot.Primary]       = ReadAction(m_Primary);
            m_Actions[(int)ControlSlot.Secondary]     = ReadAction(m_Secondary);
            m_Actions[(int)ControlSlot.Tertiary]      = ReadAction(m_Tertiary);
            m_Actions[(int)ControlSlot.Quaternary]    = ReadAction(m_Quaternary);
            m_Actions[(int)ControlSlot.ShoulderLeft]  = ReadAction(m_ShoulderLeft);
            m_Actions[(int)ControlSlot.ShoulderRight] = ReadAction(m_ShoulderRight);
            m_Actions[(int)ControlSlot.TriggerLeft]   = ReadAction(m_TriggerLeft);
            m_Actions[(int)ControlSlot.TriggerRight]  = ReadAction(m_TriggerRight);
            m_Actions[(int)ControlSlot.Start]         = ReadAction(m_Start);
            m_Actions[(int)ControlSlot.Select]        = ReadAction(m_Select);
        }

        private static InputAction ReadAction(InputActionReference reference)
        {
            InputAction action = reference != null ? reference.action : null;

            return action;
        }

        private void RouteMovement(InputAction.CallbackContext context)
        {
            m_InputRouter.RouteMovement(context.ReadValue<Vector2>());
        }
    }
}