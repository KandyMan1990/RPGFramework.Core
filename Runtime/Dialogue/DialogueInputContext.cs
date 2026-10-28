using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Dialogue
{
    /// <summary>
    /// The confirm presses read by the open dialogue windows. The newest window has the focus — the newest question while
    /// one is open, as a question holds it until answered — and only the focused window finishes typing, turns a page or
    /// answers on a press; but one press closes every window that has finished and waits to be closed. A window that
    /// waits keeps the press; with only background windows open, it carries on to what is under them. Cancel, on the
    /// focused question, moves its cursor to its cancel answer.
    /// </summary>
    public sealed class DialogueInputContext : IInputContext
    {
        private readonly List<OpenWindow> m_Windows;

        private TaskCompletionSource<bool> m_NextPress;
        private int                        m_Blockers;

        public DialogueInputContext()
        {
            m_Windows   = new List<OpenWindow>();
            m_NextPress = NewPress();
        }

        private bool IsBlocking => m_Blockers > 0;

        bool IInputContext.Handle(ControlSlot slot)
        {
            if (slot == ControlSlot.Secondary)
            {
                return HandleCancel();
            }

            if (slot != ControlSlot.Primary)
            {
                return IsBlocking;
            }

            TaskCompletionSource<bool> press = m_NextPress;
            m_NextPress = NewPress();

            OpenWindow focused = Focused();

            if (focused != null)
            {
                TaskCompletionSource<bool> focusedPress = focused.Press;
                focused.Press = NewPress();
                focusedPress.TrySetResult(true);
            }

            for (int i = 0; i < m_Windows.Count; i++)
            {
                if (m_Windows[i].PressCloses)
                {
                    m_Windows[i].PressToClose.TrySetResult(true);
                }
            }

            press.TrySetResult(true);

            return KeepsPress();
        }

        bool IInputContext.TakesMovement => IsBlocking;

        /// <summary>A window shown, which takes the focus unless a question holds it.</summary>
        public void Open(IDialogueWindow window, DialogueWindowKind kind)
        {
            m_Windows.Add(new OpenWindow(window, kind));
        }

        /// <summary>A window gone, its focus passing to the newest still open.</summary>
        public void Close(IDialogueWindow window)
        {
            m_Windows.Remove(Find(window));
        }

        /// <summary>A script waits for this window to close, so a press closes it once it has finished.</summary>
        public void LetPressClose(IDialogueWindow window)
        {
            OpenWindow open = Find(window);

            // A window still animating shut has already closed here.
            if (open != null)
            {
                open.PressCloses = true;
            }
        }

        public void BlockOtherInput()
        {
            m_Blockers++;
        }

        public void UnblockOtherInput()
        {
            m_Blockers--;
        }

        /// <summary>The next press, wherever the focus is: what closes a window that has finished.</summary>
        internal Task WaitForPressAsync()
        {
            return m_NextPress.Task;
        }

        /// <summary>The next press while the window has the focus: what finishes its typing, turns its page or answers.</summary>
        internal Task WaitForFocusedPressAsync(IDialogueWindow window)
        {
            return Find(window).Press.Task;
        }

        /// <summary>The next cancel press while the question has the focus: what moves its cursor to its cancel answer.</summary>
        internal Task WaitForCancelAsync(IDialogueWindow question)
        {
            return Find(question).Cancel.Task;
        }

        /// <summary>
        /// The next press after this call, once a script waits for the window to close: a press that turned an earlier
        /// page does not count.
        /// </summary>
        internal Task WaitForPressToCloseAsync(IDialogueWindow window)
        {
            OpenWindow open = Find(window);
            open.PressToClose = NewPress();

            return open.PressToClose.Task;
        }

        private OpenWindow Focused()
        {
            for (int i = m_Windows.Count - 1; i >= 0; i--)
            {
                if (m_Windows[i].Kind == DialogueWindowKind.Question)
                {
                    return m_Windows[i];
                }
            }

            return m_Windows.Count > 0 ? m_Windows[^1] : null;
        }

        private bool HandleCancel()
        {
            OpenWindow focused = Focused();

            if (focused == null || focused.Kind != DialogueWindowKind.Question)
            {
                return IsBlocking;
            }

            TaskCompletionSource<bool> cancel = focused.Cancel;
            focused.Cancel = NewPress();
            cancel.TrySetResult(true);

            return true;
        }

        private bool KeepsPress()
        {
            for (int i = 0; i < m_Windows.Count; i++)
            {
                if (m_Windows[i].Kind != DialogueWindowKind.Background)
                {
                    return true;
                }
            }

            return false;
        }

        private OpenWindow Find(IDialogueWindow window)
        {
            for (int i = 0; i < m_Windows.Count; i++)
            {
                if (m_Windows[i].Window == window)
                {
                    return m_Windows[i];
                }
            }

            return null;
        }

        private static TaskCompletionSource<bool> NewPress()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private sealed class OpenWindow
        {
            internal readonly IDialogueWindow    Window;
            internal readonly DialogueWindowKind Kind;

            internal TaskCompletionSource<bool> Press;
            internal TaskCompletionSource<bool> PressToClose;
            internal TaskCompletionSource<bool> Cancel;
            internal bool                       PressCloses;

            internal OpenWindow(IDialogueWindow window, DialogueWindowKind kind)
            {
                Window       = window;
                Kind         = kind;
                PressToClose = NewPress();
                Press        = NewPress();
                Cancel       = NewPress();
                PressCloses  = false;
            }
        }
    }
}
