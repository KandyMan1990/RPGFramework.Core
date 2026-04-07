using System;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;

namespace RPGFramework.Core.Dialogue.Flows
{
    public class ChoiceDialogueFlow : IDialogueFlow
    {
        async Task IDialogueFlow.RunAsync(IDialogueWindowUI uiInstance, string[] dialogues, DialogueInputContext inputContext)
        {
            DialogueBlock dialogueBlock = DialogueUtils.ParseIntoPages(dialogues[0]);

            for (int i = 0; i < dialogueBlock.Pages.Length - 1; i++)
            {
                await DialogueUtils.RunPageAsync(uiInstance, dialogueBlock.Pages[i], inputContext);
            }

            DialoguePage lastPage = dialogueBlock.Pages[^1];

            uiInstance.SetChoices(dialogues.AsSpan(1));

            await DialogueUtils.RunPageAsync(uiInstance, lastPage, inputContext);
        }
    }
}