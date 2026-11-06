using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.Flows;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenTheCursorMovesBetweenAnswers
    {
        private FakeSfxPlayer m_Sfx;

        [SetUp]
        public void SetUp()
        {
            m_Sfx = new FakeSfxPlayer();

            IDialogueFlow      flow   = new ChoiceDialogueFlow(m_Sfx, new FakeSystemSoundConfig(), 0, ChoiceDialogueFlow.NO_CANCEL_ANSWER);
            FakeDialogueWindow window = new FakeDialogueWindow(new TaskCompletionSource<bool>().Task);

            // Left waiting on an answer that never comes, as the question stays open.
            flow.RunAsync(window, new[] { "Stay or go?", "Stay", "Go" }, Presses.ContextWith(window, DialogueWindowKind.Question), CancellationToken.None);

            window.MoveCursor();
            window.MoveCursor();
        }

        [Test]
        public void ThenTheCursorSoundPlaysForEachMove()
        {
            Assert.AreEqual(2, m_Sfx.Played.Count);
            Assert.AreEqual(FakeSystemSoundConfig.CURSOR, m_Sfx.Played[0]);
            Assert.AreEqual(FakeSystemSoundConfig.CURSOR, m_Sfx.Played[1]);
        }
    }
}
