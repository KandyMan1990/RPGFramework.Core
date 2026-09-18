using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.Core.Input
{
    public interface IInputRouter
    {
        void          Push(IInputContext context);
        IInputContext Pop(IInputContext  context);
        void          Clear();
        void          Route(ControlSlot     slot);
        void          RouteMovement(Vector2 move);
    }

    internal sealed class InputRouter : IInputRouter
    {
        private readonly List<IInputContext> m_Stack;

        public InputRouter()
        {
            m_Stack = new List<IInputContext>();
        }

        void IInputRouter.Push(IInputContext context)
        {
            m_Stack.Add(context);
        }

        IInputContext IInputRouter.Pop(IInputContext context)
        {
            int index = m_Stack.LastIndexOf(context);

            if (index >= 0)
            {
                m_Stack.RemoveAt(index);
            }

            IInputContext top = m_Stack.Count == 0 ? null : m_Stack[^1];

            return top;
        }

        void IInputRouter.Clear()
        {
            m_Stack.Clear();
        }

        void IInputRouter.Route(ControlSlot slot)
        {
            for (int i = m_Stack.Count - 1; i >= 0; i--)
            {
                if (m_Stack[i].Handle(slot))
                {
                    break;
                }
            }
        }

        void IInputRouter.RouteMovement(Vector2 move)
        {
            for (int i = m_Stack.Count - 1; i >= 0; i--)
            {
                if (m_Stack[i].HandleMove(move))
                {
                    break;
                }
            }
        }
    }
}