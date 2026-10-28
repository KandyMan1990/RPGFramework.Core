using System.Threading;
using System.Threading.Tasks;

namespace RPGFramework.Core.Dialogue.Flows
{
    public sealed class TextDialogueFlow : IDialogueFlow
    {
        DialogueWindowKind IDialogueFlow.Kind => DialogueWindowKind.Message;

        async Task IDialogueFlow.RunAsync(IDialogueWindow window, string[] dialogues, DialogueInputContext inputContext, CancellationToken close)
        {
            for (int i = 0; i < dialogues.Length; i++)
            {
                DialogueBlock dialogueBlock = DialogueUtils.ParseIntoPages(dialogues[i]);

                for (int j = 0; j < dialogueBlock.Pages.Length && !close.IsCancellationRequested; j++)
                {
                    bool isLast = i == dialogues.Length - 1 && j == dialogueBlock.Pages.Length - 1;

                    await DialogueUtils.RunPageAsync(window, dialogueBlock.Pages[j], inputContext, close, isLast);
                }
            }
        }
    }
}