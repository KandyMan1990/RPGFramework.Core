using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.Flows;
using RPGFramework.Core.Input;
using UnityEngine.TestTools;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>
    /// Cancel on a question with a cancel answer moves the cursor there, with the cancel sound, and does not answer it;
    /// on one with none, it does nothing.
    /// </summary>
    internal sealed class WhenCancelIsPressedOnAQuestion
    {
        private const int FRAMES = 30;

        private FakeSfxPlayer      m_Sfx;
        private FakeDialogueWindow m_Window;
        private Task               m_Asked;

        [UnityTest]
        public IEnumerator ThenTheCursorMovesToTheCancelAnswer()
        {
            yield return Cancel(2);

            Assert.AreEqual(2, m_Window.MovedTo);
        }

        [UnityTest]
        public IEnumerator ThenTheCancelSoundPlays()
        {
            yield return Cancel(2);

            Assert.AreEqual(1, m_Sfx.Played.Count);
            Assert.AreEqual(FakeSystemSoundConfig.CANCEL, m_Sfx.Played[0]);
        }

        [UnityTest]
        public IEnumerator ThenTheQuestionIsStillWaitingForAnAnswer()
        {
            yield return Cancel(2);

            Assert.IsFalse(m_Asked.IsCompleted);
        }

        [UnityTest]
        public IEnumerator ThenWithNoCancelAnswerNothingHappens()
        {
            yield return Cancel(ChoiceDialogueFlow.NO_CANCEL_ANSWER);

            Assert.AreEqual(FakeDialogueWindow.NOT_MOVED, m_Window.MovedTo);
            Assert.AreEqual(0, m_Sfx.Played.Count);
        }

        private IEnumerator Cancel(byte cancelAnswer)
        {
            m_Sfx    = new FakeSfxPlayer();
            m_Window = new FakeDialogueWindow(new TaskCompletionSource<bool>().Task);

            IDialogueFlow        flow    = new ChoiceDialogueFlow(m_Sfx, new FakeSystemSoundConfig(), 0, cancelAnswer);
            DialogueInputContext context = Presses.ContextWith(m_Window, DialogueWindowKind.Question);

            m_Asked = flow.RunAsync(m_Window, new[] { "Stay, go or wait?", "Stay", "Go", "Wait" }, context, CancellationToken.None);

            ((IInputContext)context).Handle(CoreActions.Cancel);

            // The flow carries on from the press on a later frame, as the editor's main thread runs its continuations.
            for (int i = 0; i < FRAMES && m_Window.MovedTo == FakeDialogueWindow.NOT_MOVED; i++)
            {
                yield return null;
            }
        }
    }
}
