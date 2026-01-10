using System.Collections.Generic;

namespace RPGFramework.Core.Input
{
    public interface IInputRouter
    {
        void Push(IInputContext context);
        void Pop(IInputContext  context);
        void Clear();
        void Route(ControlSlot slot);
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

        void IInputRouter.Pop(IInputContext context)
        {
            if (m_Stack.Count > 0 && m_Stack.Peek() == context)
            {
                m_Stack.Pop();
            }
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
    }
}