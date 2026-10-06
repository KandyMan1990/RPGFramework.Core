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
        private readonly IDialogueWindowUI m_UIInstance;

        public DialogueWindow(IDialogueWindowUI uiInstance)
        {
            m_UIInstance = uiInstance;
        }

        Task IDialogueWindow.AnimateWindowClosedAsync()
        {
            return m_UIInstance.AnimateWindowClosedAsync();
        }

        Task IDialogueWindow.AnimateWindowOpenAsync()
        {
            return m_UIInstance.AnimateWindowOpenAsync();
        }

        void IDialogueWindow.Destroy()
        {
            m_UIInstance.Destroy();
        }

        byte IDialogueWindow.GetSelectedChoice()
        {
            return m_UIInstance.GetSelectedChoice();
        }

        void IDialogueWindow.Init(VisualElement container)
        {
            m_UIInstance.Init(container);
        }

        Task IDialogueWindow.RunAsync(IDialogueFlow dialogueFlow, string[] dialogues, DialogueInputContext inputContext, CancellationToken close)
        {
            return dialogueFlow.RunAsync(m_UIInstance, dialogues, inputContext, close);
        }

        void IDialogueWindow.SetMessageVariables(IReadOnlyList<int> variables)
        {
            m_UIInstance.SetMessageVariables(variables);
        }

        void IDialogueWindow.SetMessageSpeed(float messageSpeed)
        {
            m_UIInstance.SetMessageSpeed(messageSpeed);
        }

        void IDialogueWindow.SetRect(RectInt rect)
        {
            m_UIInstance.SetRect(rect);
        }

        void IDialogueWindow.SetStyle(DialogueWindowStyle style)
        {
            m_UIInstance.SetStyle(style);
        }
    }
}