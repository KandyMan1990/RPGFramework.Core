using System.Threading.Tasks;
using RPGFramework.Core.Input;
using UnityEngine;

namespace RPGFramework.Core.DialogueWindow
{
    public class DialogueInputContext : IInputContext
    {
        private TaskCompletionSource<bool> m_OnAdvance;

        bool IInputContext.Handle(ControlSlot slot)
        {
            switch (slot)
            {
                case ControlSlot.Primary:
                    m_OnAdvance.TrySetResult(true);
                    return true;
            }

            return false;
        }

        void IInputContext.HandleMove(Vector2 move)
        {
            // noop
        }

        public Task WaitForConfirmAsync()
        {
            return m_OnAdvance.Task;
        }

        public void Reset()
        {
            m_OnAdvance?.TrySetCanceled();
            m_OnAdvance = new TaskCompletionSource<bool>();
        }
    }
}