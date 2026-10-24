using System;
using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Core.Audio;

namespace RPGFramework.Core.Dialogue.Flows
{
    /// <summary>
    /// Pages ending on a question. It sounds the cursor moving between the answers and the one chosen; a question a
    /// script closes is not answered, and makes no sound.
    /// </summary>
    public sealed class ChoiceDialogueFlow : IDialogueFlow
    {
        private readonly IAudioIntentPlayer m_AudioIntentPlayer;
        private readonly AudioContext       m_Context;

        public ChoiceDialogueFlow(IAudioIntentPlayer audioIntentPlayer, AudioContext context)
        {
            m_AudioIntentPlayer = audioIntentPlayer;
            m_Context           = context;
        }

        async Task IDialogueFlow.RunAsync(IDialogueWindow window, string[] dialogues, DialogueInputContext inputContext, CancellationToken close)
        {
            DialogueBlock dialogueBlock = DialogueUtils.ParseIntoPages(dialogues[0]);

            for (int i = 0; i < dialogueBlock.Pages.Length - 1; i++)
            {
                await DialogueUtils.RunPageAsync(window, dialogueBlock.Pages[i], inputContext, close);

                if (close.IsCancellationRequested)
                {
                    return;
                }
            }

            DialoguePage lastPage = dialogueBlock.Pages[^1];

            window.SetChoices(dialogues.AsSpan(1));
            window.OnCursorMoved += OnCursorMoved;

            bool answered = await DialogueUtils.RunQuestionPageAsync(window, lastPage, inputContext, close);

            window.OnCursorMoved -= OnCursorMoved;

            if (answered)
            {
                m_AudioIntentPlayer.PlayAsync(AudioIntent.Confirm, m_Context).FireAndForget();
            }
        }

        private void OnCursorMoved()
        {
            m_AudioIntentPlayer.PlayAsync(AudioIntent.Navigate, m_Context).FireAndForget();
        }
    }
}