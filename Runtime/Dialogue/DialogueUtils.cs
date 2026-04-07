using System;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;

namespace RPGFramework.Core.Dialogue
{
    internal static class DialogueUtils
    {
        private const string NEW_PAGE_MARKER = "{NewPage}";

        internal static DialogueBlock ParseIntoPages(string dialogue)
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

        internal static async Task RunPageAsync(IDialogueWindowUI uiInstance, DialoguePage dialoguePage, DialogueInputContext inputContext)
        {
            uiInstance.SetText(dialoguePage);

            inputContext.Reset();

            Task animationTask = uiInstance.RunAsync();
            Task confirmTask   = inputContext.WaitForConfirmAsync();

            Task completed = await Task.WhenAny(animationTask, confirmTask);

            if (completed == confirmTask)
            {
                uiInstance.SkipToAnimationEnd();
                await animationTask;
            }

            inputContext.Reset();
            await inputContext.WaitForConfirmAsync();
        }
    }
}