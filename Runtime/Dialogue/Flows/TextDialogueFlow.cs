using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;

namespace RPGFramework.Core.Dialogue.Flows
{
    public class TextDialogueFlow : IDialogueFlow
    {
        async Task IDialogueFlow.RunAsync(IDialogueWindowUI uiInstance, string[] dialogues, DialogueInputContext inputContext)
        {
            for (int i = 0; i < dialogues.Length; i++)
            {
                DialogueBlock dialogueBlock = DialogueUtils.ParseIntoPages(dialogues[i]);

                for (int j = 0; j < dialogueBlock.Pages.Length; j++)
                {
                    await DialogueUtils.RunPageAsync(uiInstance, dialogueBlock.Pages[j], inputContext);
                }
            }
        }
    }
}