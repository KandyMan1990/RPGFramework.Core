using System;
using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Core.Audio;

namespace RPGFramework.Core.Dialogue.Flows
{
    /// <summary>
    /// Pages ending on a question, its cursor starting on one answer, and cancel moving it to another — the player still
    /// confirms it. It sounds the cursor moving between the answers, cancel moving it and the one chosen; a question a
    /// script closes is not answered, and makes no sound.
    /// </summary>
    public sealed class ChoiceDialogueFlow : IDialogueFlow
    {
        /// <summary>A cancel answer that is none: cancel leaves the cursor where it is.</summary>
        public const byte NO_CANCEL_ANSWER = byte.MaxValue;

        private readonly IAudioIntentPlayer m_AudioIntentPlayer;
        private readonly AudioContext       m_Context;
        private readonly byte               m_StartingAnswer;
        private readonly byte               m_CancelAnswer;

        public ChoiceDialogueFlow(IAudioIntentPlayer audioIntentPlayer, AudioContext context, byte startingAnswer, byte cancelAnswer)
        {
            m_AudioIntentPlayer = audioIntentPlayer;
            m_Context           = context;
            m_StartingAnswer    = startingAnswer;
            m_CancelAnswer      = cancelAnswer;
        }

        DialogueWindowKind IDialogueFlow.Kind => DialogueWindowKind.Question;

        async Task IDialogueFlow.RunAsync(IDialogueWindow window, string[] dialogues, DialogueInputContext inputContext, CancellationToken close)
        {
            DialogueBlock dialogueBlock = DialogueUtils.ParseIntoPages(dialogues[0]);

            for (int i = 0; i < dialogueBlock.Pages.Length - 1; i++)
            {
                await DialogueUtils.RunPageAsync(window, dialogueBlock.Pages[i], inputContext, close, false);

                if (close.IsCancellationRequested)
                {
                    return;
                }
            }

            DialoguePage lastPage = dialogueBlock.Pages[^1];

            window.SetChoices(dialogues.AsSpan(1), m_StartingAnswer);
            window.OnCursorMoved += OnCursorMoved;

            Action cancelled = m_CancelAnswer == NO_CANCEL_ANSWER ? null : () => MoveToCancelAnswer(window);
            bool   answered  = await DialogueUtils.RunQuestionPageAsync(window, lastPage, inputContext, close, cancelled);

            window.OnCursorMoved -= OnCursorMoved;

            if (answered)
            {
                m_AudioIntentPlayer.PlayAsync(AudioIntent.Confirm, m_Context).FireAndForget();
            }
        }

        private void MoveToCancelAnswer(IDialogueWindow window)
        {
            window.FocusChoice(m_CancelAnswer);

            m_AudioIntentPlayer.PlayAsync(AudioIntent.Cancel, m_Context).FireAndForget();
        }

        private void OnCursorMoved()
        {
            m_AudioIntentPlayer.PlayAsync(AudioIntent.Navigate, m_Context).FireAndForget();
        }
    }
}