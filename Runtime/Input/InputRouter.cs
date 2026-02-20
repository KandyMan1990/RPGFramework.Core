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

    public sealed class InputRouter : IInputRouter
    {
        private readonly Stack<IInputContext> m_Stack;

        public InputRouter()
        {
            m_Stack = new Stack<IInputContext>();
        }

        void IInputRouter.Push(IInputContext context)
        {
            m_Stack.Push(context);
        }

        IInputContext IInputRouter.Pop(IInputContext context)
        {
            if (m_Stack.Count > 0 && m_Stack.Peek() == context)
            {
                return m_Stack.Pop();
            }

            return null;
        }

        void IInputRouter.Clear()
        {
            m_Stack.Clear();
        }

        void IInputRouter.Route(ControlSlot slot)
        {
            foreach (IInputContext context in m_Stack)
            {
                if (context.Handle(slot))
                {
                    break;
                }
            }
        }

        void IInputRouter.RouteMovement(Vector2 move)
        {
            if (m_Stack.Count == 0)
            {
                return;
            }
            
            m_Stack.Peek().HandleMove(move);
        }
    }
}