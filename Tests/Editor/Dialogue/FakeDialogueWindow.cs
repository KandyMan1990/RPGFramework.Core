using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>
    /// A window that shows everything at once, answered when the choice it is given completes, noting where its cursor
    /// starts and where it is moved.
    /// </summary>
    internal sealed class FakeDialogueWindow : IDialogueWindow
    {
        event Action IDialogueWindow.OnCursorMoved
        {
            add => m_OnCursorMoved += value;
            remove => m_OnCursorMoved -= value;
        }

        internal const int NOT_MOVED = -1;

        private readonly Task m_Choice;

        private byte m_StartingChoice;
        private int  m_MovedTo;

        private event Action m_OnCursorMoved;

        internal FakeDialogueWindow(Task choice)
        {
            m_Choice         = choice;
            m_StartingChoice = 0;
            m_MovedTo        = NOT_MOVED;
        }

        internal byte StartingChoice => m_StartingChoice;

        /// <summary>The choice the cursor was last moved to, or <see cref="NOT_MOVED" />.</summary>
        internal int MovedTo => m_MovedTo;

        /// <summary>The player moving the cursor to another answer.</summary>
        internal void MoveCursor()
        {
            m_OnCursorMoved?.Invoke();
        }

        Task IDialogueWindow.AnimateWindowClosedAsync() => Task.CompletedTask;

        Task IDialogueWindow.AnimateWindowOpenAsync() => Task.CompletedTask;

        void IDialogueWindow.Destroy() { }

        byte IDialogueWindow.GetSelectedChoice() => 0;

        void IDialogueWindow.ChooseFocusedChoice() { }

        Task IDialogueWindow.WaitForChoiceAsync() => m_Choice;

        void IDialogueWindow.Init(VisualElement container) { }

        Task IDialogueWindow.RunAsync() => Task.CompletedTask;

        void IDialogueWindow.SetChoices(ReadOnlySpan<string> choices, byte startingChoice) => m_StartingChoice = startingChoice;

        void IDialogueWindow.FocusChoice(byte index) => m_MovedTo = index;

        void IDialogueWindow.SetMessageVariables(IReadOnlyList<int> variables) { }

        void IDialogueWindow.SetMessageSpeed(float messageSpeed) { }

        void IDialogueWindow.SetRect(RectInt rect) { }

        void IDialogueWindow.SetStyle(DialogueWindowStyle style) { }

        void IDialogueWindow.SetText(DialoguePage dialoguePage) { }

        void IDialogueWindow.SkipToAnimationEnd() { }
    }
}
