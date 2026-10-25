using System.Collections.Generic;

namespace RPGFramework.Core.Input
{
    /// <summary>
    /// A stack of <see cref="IInputContext" />s, each press offered to the top one first and on down until one takes
    /// it, so whatever opened last — a dialogue window over the field, a menu over a menu — gets input first. Movement
    /// is not routed: what moves reads it each tick, when <see cref="MovementReaches" /> says it may.
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

        /// <summary>
        /// True if movement reaches <paramref name="context" />: input is not locked, and no context above it takes
        /// movement. False for a context not on the stack.
        /// </summary>
        bool MovementReaches(IInputContext context);
    }

    /// <summary>
    /// Holds routed input back while it must not act — while the screen fades, so nothing pressed during a change starts
    /// another. Locks nest; input flows again once every one is released.
    /// </summary>
    internal interface IInputLock
    {
        /// <summary>Holds input back: no press is routed, and movement reaches no context.</summary>
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

        bool IInputRouter.MovementReaches(IInputContext context)
        {
            if (m_Locks > 0)
            {
                return false;
            }

            for (int i = m_Stack.Count - 1; i >= 0; i--)
            {
                if (m_Stack[i] == context)
                {
                    return true;
                }

                if (m_Stack[i].TakesMovement)
                {
                    return false;
                }
            }

            return false;
        }

        void IInputLock.Lock()
        {
            m_Locks++;
        }

        void IInputLock.Unlock()
        {
            if (m_Locks > 0)
            {
                m_Locks--;
            }
        }
    }
}