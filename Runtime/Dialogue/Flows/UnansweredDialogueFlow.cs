using System.Threading;
using System.Threading.Tasks;

namespace RPGFramework.Core.Dialogue.Flows
{
    /// <summary>
    /// A window the player never answers, left up while they carry on: they walk, talk to someone or open the menu while
    /// it plays out, a press reaching what is under it. It turns its pages on a press while it has the focus, and its
    /// last stays up until a script closes it — or, once the script waits for it, until a press does.
    /// </summary>
    public sealed class UnansweredDialogueFlow : IDialogueFlow
    {
        DialogueWindowKind IDialogueFlow.Kind => DialogueWindowKind.Background;

        async Task IDialogueFlow.RunAsync(IDialogueWindow window, string[] dialogues, DialogueInputContext inputContext, CancellationToken close)
        {
            for (int i = 0; i < dialogues.Length; i++)
            {
                DialogueBlock dialogueBlock = DialogueUtils.ParseIntoPages(dialogues[i]);

                for (int j = 0; j < dialogueBlock.Pages.Length; j++)
                {
                    bool isLast = i == dialogues.Length - 1 && j == dialogueBlock.Pages.Length - 1;

                    if (!await DialogueUtils.RunBackgroundPageAsync(window, dialogueBlock.Pages[j], inputContext, close, isLast))
                    {
                        return;
                    }
                }
            }
        }
    }
}
