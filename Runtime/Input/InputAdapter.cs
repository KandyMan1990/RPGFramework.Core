using RPGFramework.DI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Input
{
    public sealed class InputAdapter : MonoBehaviour
    {
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

        [Inject]
        public void Inject(IInputRouter inputRouter)
        {
            m_InputRouter = inputRouter;
        }

        public void Enable()
        {
            if (m_Movement != null)
            {
                m_Movement.action.performed += RouteMovement;
                m_Movement.action.canceled  += RouteMovement;
                m_Movement.action.Enable();
            }
            if (m_Primary != null)
            {
                m_Primary.action.performed += RoutePrimary;
                m_Primary.action.Enable();
            }
            if (m_Secondary != null)
            {
                m_Secondary.action.performed += RouteSecondary;
                m_Secondary.action.Enable();
            }
            if (m_Tertiary != null)
            {
                m_Tertiary.action.performed += RouteTertiary;
                m_Tertiary.action.Enable();
            }
            if (m_Quaternary != null)
            {
                m_Quaternary.action.performed += RouteQuaternary;
                m_Quaternary.action.Enable();
            }
            if (m_ShoulderLeft != null)
            {
                m_ShoulderLeft.action.performed += RouteShoulderLeft;
                m_ShoulderLeft.action.Enable();
            }
            if (m_ShoulderRight != null)
            {
                m_ShoulderRight.action.performed += RouteShoulderRight;
                m_ShoulderRight.action.Enable();
            }
            if (m_TriggerLeft != null)
            {
                m_TriggerLeft.action.performed += RouteTriggerLeft;
                m_TriggerLeft.action.Enable();
            }
            if (m_TriggerRight != null)
            {
                m_TriggerRight.action.performed += RouteTriggerRight;
                m_TriggerRight.action.Enable();
            }
            if (m_Start != null)
            {
                m_Start.action.performed += RouteStart;
                m_Start.action.Enable();
            }
            if (m_Select != null)
            {
                m_Select.action.performed += RouteSelect;
                m_Select.action.Enable();
            }
        }

        public void Disable()
        {
            if (m_Movement != null)
            {
                m_Movement.action.Disable();
                m_Movement.action.canceled  -= RouteMovement;
                m_Movement.action.performed -= RouteMovement;
            }
            if (m_Primary != null)
            {
                m_Primary.action.Disable();
                m_Primary.action.performed -= RoutePrimary;
            }
            if (m_Secondary != null)
            {
                m_Secondary.action.Disable();
                m_Secondary.action.performed -= RouteSecondary;
            }
            if (m_Tertiary != null)
            {
                m_Tertiary.action.Disable();
                m_Tertiary.action.performed -= RouteTertiary;
            }
            if (m_Quaternary != null)
            {
                m_Quaternary.action.Disable();
                m_Quaternary.action.performed -= RouteQuaternary;
            }
            if (m_ShoulderLeft != null)
            {
                m_ShoulderLeft.action.Disable();
                m_ShoulderLeft.action.performed -= RouteShoulderLeft;
            }
            if (m_ShoulderRight != null)
            {
                m_ShoulderRight.action.Disable();
                m_ShoulderRight.action.performed -= RouteShoulderRight;
            }
            if (m_TriggerLeft != null)
            {
                m_TriggerLeft.action.Disable();
                m_TriggerLeft.action.performed -= RouteTriggerLeft;
            }
            if (m_TriggerRight != null)
            {
                m_TriggerRight.action.Disable();
                m_TriggerRight.action.performed -= RouteTriggerRight;
            }
            if (m_Start != null)
            {
                m_Start.action.Disable();
                m_Start.action.performed -= RouteStart;
            }
            if (m_Select != null)
            {
                m_Select.action.Disable();
                m_Select.action.performed -= RouteSelect;
            }
        }

        private void RouteMovement(InputAction.CallbackContext context)
        {
            m_InputRouter.RouteMovement(context.ReadValue<Vector2>());
        }

        private void RoutePrimary(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.Primary);
        }

        private void RouteSecondary(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.Secondary);
        }

        private void RouteTertiary(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.Tertiary);
        }

        private void RouteQuaternary(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.Quaternary);
        }

        private void RouteShoulderLeft(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.ShoulderLeft);
        }

        private void RouteShoulderRight(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.ShoulderRight);
        }

        private void RouteTriggerLeft(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.TriggerLeft);
        }

        private void RouteTriggerRight(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.TriggerRight);
        }

        private void RouteStart(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.Start);
        }

        private void RouteSelect(InputAction.CallbackContext context)
        {
            m_InputRouter.Route(ControlSlot.Select);
        }
    }
}