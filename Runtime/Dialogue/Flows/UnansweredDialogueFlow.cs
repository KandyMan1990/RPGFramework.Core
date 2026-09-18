using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;

namespace RPGFramework.Core.Dialogue.Flows
{
    /// <summary>
    /// A window the player never answers: it types its line and stays up until it is closed. Nothing the player
    /// presses reaches it, which is what lets a conversation play out in the background while they walk, talk to
    /// someone or open the menu; the script decides when it goes, by waiting or by whatever else it watches for.
    /// <br /><br />
    /// Nothing can turn a page, so a line with several is shown whole, its page breaks shown as line breaks.
    /// </summary>
    public sealed class UnansweredDialogueFlow : IDialogueFlow
    {
        async Task IDialogueFlow.RunAsync(IDialogueWindowUI uiInstance, string[] dialogues, DialogueInputContext inputContext, CancellationToken close)
        {
            foreach (string dialogue in dialogues)
            {
                DialoguePage page = DialogueUtils.ParseIntoPages(DialogueUtils.JoinPages(dialogue)).Pages[0];

                await DialogueUtils.RunUnansweredPageAsync(uiInstance, page, close);
            }
        }
    }
}