using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue
{
    internal sealed class DialogueWindow : IDialogueWindow
    {
        private readonly IDialogueWindowUI m_UiInstance;

        public DialogueWindow(IDialogueWindowUI uiInstance)
        {
            m_UiInstance = uiInstance;
        }

        Task IDialogueWindow.AnimateWindowClosedAsync()
        {
            return m_UiInstance.AnimateWindowClosedAsync();
        }

        Task IDialogueWindow.AnimateWindowOpenAsync()
        {
            return m_UiInstance.AnimateWindowOpenAsync();
        }

        void IDialogueWindow.Destroy()
        {
            m_UiInstance.Destroy();
        }

        byte IDialogueWindow.GetSelectedChoice()
        {
            return m_UiInstance.GetSelectedChoice();
        }

        void IDialogueWindow.Init(VisualElement container)
        {
            m_UiInstance.Init(container);
        }

        Task IDialogueWindow.RunAsync(IDialogueFlow dialogueFlow, string[] dialogues, DialogueInputContext inputContext, CancellationToken close)
        {
            return dialogueFlow.RunAsync(m_UiInstance, dialogues, inputContext, close);
        }

        void IDialogueWindow.SetMessageVariables(IReadOnlyList<int> variables)
        {
            m_UiInstance.SetMessageVariables(variables);
        }

        void IDialogueWindow.SetRect(RectInt rect)
        {
            m_UiInstance.SetRect(rect);
        }

        void IDialogueWindow.SetStyle(DialogueWindowStyle style)
        {
            m_UiInstance.SetStyle(style);
        }
    }
}