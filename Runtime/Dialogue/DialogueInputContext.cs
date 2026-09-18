using System.Threading.Tasks;
using RPGFramework.Core.Input;
using UnityEngine;

namespace RPGFramework.Core.Dialogue
{
    public sealed class DialogueInputContext : IInputContext
    {
        private TaskCompletionSource<bool> m_NextConfirm = NewConfirm();
        private int                        m_Blockers;

        private bool IsBlocking => m_Blockers > 0;

        bool IInputContext.Handle(ControlSlot slot)
        {
            if (slot == ControlSlot.Primary)
            {
                TaskCompletionSource<bool> confirm = m_NextConfirm;

                m_NextConfirm = NewConfirm();
                confirm.TrySetResult(true);

                return true;
            }

            return IsBlocking;
        }

        bool IInputContext.HandleMove(Vector2 move)
        {
            return IsBlocking;
        }

        public Task WaitForConfirmAsync()
        {
            Task next = m_NextConfirm.Task;

            return next;
        }

        public void BlockOtherInput()
        {
            m_Blockers++;
        }

        public void UnblockOtherInput()
        {
            m_Blockers--;
        }

        private static TaskCompletionSource<bool> NewConfirm()
        {
            TaskCompletionSource<bool> confirm = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            return confirm;
        }
    }
}