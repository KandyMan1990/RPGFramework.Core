using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>A window that shows everything at once, answered when the choice it is given completes.</summary>
    internal sealed class FakeDialogueWindow : IDialogueWindow
    {
        event Action IDialogueWindow.OnCursorMoved
        {
            add => m_OnCursorMoved += value;
            remove => m_OnCursorMoved -= value;
        }

        private readonly Task m_Choice;

        private event Action m_OnCursorMoved;

        internal FakeDialogueWindow(Task choice)
        {
            m_Choice = choice;
        }

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

        void IDialogueWindow.SetChoices(ReadOnlySpan<string> choices) { }

        void IDialogueWindow.SetMessageVariables(IReadOnlyList<int> variables) { }

        void IDialogueWindow.SetMessageSpeed(float messageSpeed) { }

        void IDialogueWindow.SetRect(RectInt rect) { }

        void IDialogueWindow.SetStyle(DialogueWindowStyle style) { }

        void IDialogueWindow.SetText(DialoguePage dialoguePage) { }

        void IDialogueWindow.SkipToAnimationEnd() { }
    }
}
