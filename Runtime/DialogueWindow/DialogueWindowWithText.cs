using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.DialogueWindow.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.DialogueWindow
{
    public class DialogueWindowWithText : IDialogueWindowWithText
    {
        private readonly IDialogueWindowWithTextUI m_UiInstance;

        public DialogueWindowWithText(IDialogueWindowWithTextUI uiInstance)
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

        void IDialogueWindow.Init(VisualElement container, ulong id)
        {
            m_UiInstance.Init(container, id);
        }

        async Task IDialogueWindow.RunAsync(string dialogue, DialogueInputContext inputContext)
        {
            List<string> pages = ParseIntoPages(dialogue);

            for (int i = 0; i < pages.Count; i++)
            {
                await RunPageAsync(pages[i], inputContext);
            }
        }

        void IDialogueWindow.SetRect(RectInt rect)
        {
            m_UiInstance.SetRect(rect);
        }

        // TODO: separation of pages not implemented
        private List<string> ParseIntoPages(string dialogue)
        {
            List<string> pages = new List<string>();

            pages.Add(dialogue);

            return pages;
        }

        private async Task RunPageAsync(string dialogue, DialogueInputContext inputContext)
        {
            m_UiInstance.SetText(dialogue);
            
            inputContext.Reset();
            
            Task animationTask = m_UiInstance.RunAsync();
            Task confirmTask   = inputContext.WaitForConfirmAsync();
            
            Task completed = await Task.WhenAny(animationTask, confirmTask);
            
            if (completed == confirmTask)
            {
                m_UiInstance.SkipToAnimationEnd();
                await animationTask;
            }

            inputContext.Reset();
            await inputContext.WaitForConfirmAsync();
        }
    }
}