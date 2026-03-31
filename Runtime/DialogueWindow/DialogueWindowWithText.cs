using System;
using System.Threading.Tasks;
using RPGFramework.Core.DialogueWindow.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.DialogueWindow
{
    public class DialogueWindowWithText : IDialogueWindowWithText
    {
        private const string NEW_PAGE_MARKER = "{NewPage}";

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
            DialogueBlock dialogueBlock = ParseIntoPages(dialogue);

            for (int i = 0; i < dialogueBlock.Pages.Length; i++)
            {
                await RunPageAsync(dialogueBlock.Pages[i], inputContext);
            }
        }

        void IDialogueWindow.SetRect(RectInt rect)
        {
            m_UiInstance.SetRect(rect);
        }

        private static DialogueBlock ParseIntoPages(string dialogue)
        {
            string[]      pages = dialogue.Split(NEW_PAGE_MARKER);
            DialogueBlock block = new DialogueBlock(pages.Length);

            for (int i = 0; i < pages.Length; i++)
            {
                string trimmed = pages[i].Trim();

                string speaker = string.Empty;
                if (trimmed.StartsWith("["))
                {
                    int end = trimmed.IndexOf("]", StringComparison.Ordinal);
                    if (end > 0)
                    {
                        speaker = trimmed[1..end];
                        trimmed = trimmed[(end + 1)..].TrimStart();
                    }
                }

                DialoguePage item = new DialoguePage(speaker, trimmed);
                block.AddPage(i, item);
            }

            return block;
        }

        private async Task RunPageAsync(DialoguePage dialoguePage, DialogueInputContext inputContext)
        {
            m_UiInstance.SetText(dialoguePage);

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