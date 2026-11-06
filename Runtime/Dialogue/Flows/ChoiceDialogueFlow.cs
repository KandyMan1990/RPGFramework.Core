using System;
using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Audio;
using RPGFramework.Core.Audio;

namespace RPGFramework.Core.Dialogue.Flows
{
    /// <summary>
    /// Pages ending on a question, its cursor starting on one answer, and cancel moving it to another — the player still
    /// confirms it. It plays the game's system sounds as the cursor moves between the answers, as cancel moves it and
    /// as one is chosen; a question a script closes is not answered, and makes no sound.
    /// </summary>
    public sealed class ChoiceDialogueFlow : IDialogueFlow
    {
        /// <summary>A cancel answer that is none: cancel leaves the cursor where it is.</summary>
        public const byte NO_CANCEL_ANSWER = byte.MaxValue;

        private readonly ISfxPlayer         m_SfxPlayer;
        private readonly ISystemSoundConfig m_SystemSounds;
        private readonly byte               m_StartingAnswer;
        private readonly byte               m_CancelAnswer;

        public ChoiceDialogueFlow(ISfxPlayer sfxPlayer, ISystemSoundConfig systemSounds, byte startingAnswer, byte cancelAnswer)
        {
            m_SfxPlayer      = sfxPlayer;
            m_SystemSounds   = systemSounds;
            m_StartingAnswer = startingAnswer;
            m_CancelAnswer   = cancelAnswer;
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
                PlaySound(m_SystemSounds.Confirm);
            }
        }

        private void MoveToCancelAnswer(IDialogueWindow window)
        {
            window.FocusChoice(m_CancelAnswer);

            PlaySound(m_SystemSounds.Cancel);
        }

        private void OnCursorMoved()
        {
            PlaySound(m_SystemSounds.Cursor);
        }

        private void PlaySound(ulong sound)
        {
            if (sound != 0)
            {
                m_SfxPlayer.PlayAsync(sound).FireAndForget();
            }
        }
    }
}