using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.Core.Input
{
    /// <summary>
    /// A stack of <see cref="IInputContext" />s, each press offered to the top one first and on down until one takes
    /// it, so whatever opened last — a dialogue window over the field, a menu over a menu — gets input first.
    /// </summary>
    public interface IInputRouter
    {
        /// <summary>Puts a context on top, so it is offered input first.</summary>
        void Push(IInputContext context);

        /// <summary>
        /// Takes the context off, from wherever it is in the stack, and returns the one now on top, or null.
        /// </summary>
        IInputContext Pop(IInputContext context);

        /// <summary>Empties the stack.</summary>
        void Clear();

        /// <summary>Offers a button press to each context from the top down, until one takes it.</summary>
        void Route(ControlSlot slot);

        /// <summary>Offers the move input to each context from the top down, until one takes it.</summary>
        void RouteMovement(Vector2 move);
    }

    /// <summary>
    /// Holds routed input back while it must not act — while the screen fades, so nothing pressed during a change starts
    /// another. Locks nest; input flows again once every one is released.
    /// </summary>
    internal interface IInputLock
    {
        /// <summary>Holds input back; the first lock also stops whatever was moving.</summary>
        void Lock();

        /// <summary>Releases one lock.</summary>
        void Unlock();
    }

    internal sealed class InputRouter : IInputRouter, IInputLock
    {
        private readonly List<IInputContext> m_Stack;

        private int m_Locks;

        public InputRouter()
        {
            m_Stack = new List<IInputContext>();
            m_Locks = 0;
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
            if (m_Locks > 0)
            {
                return;
            }

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
            if (m_Locks > 0)
            {
                return;
            }

            RouteMove(move);
        }

        // A move held as the lock begins would otherwise carry on unseen, as its release is held back too.
        void IInputLock.Lock()
        {
            m_Locks++;

            if (m_Locks == 1)
            {
                RouteMove(Vector2.zero);
            }
        }

        void IInputLock.Unlock()
        {
            if (m_Locks > 0)
            {
                m_Locks--;
            }
        }

        private void RouteMove(Vector2 move)
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