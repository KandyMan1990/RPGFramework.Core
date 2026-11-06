using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.Flows;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenAQuestionIsAnswered
    {
        private FakeSfxPlayer m_Sfx;

        [SetUp]
        public void SetUp()
        {
            m_Sfx = new FakeSfxPlayer();

            IDialogueFlow      flow   = new ChoiceDialogueFlow(m_Sfx, new FakeSystemSoundConfig(), 0, ChoiceDialogueFlow.NO_CANCEL_ANSWER);
            FakeDialogueWindow window = new FakeDialogueWindow(Task.CompletedTask);

            flow.RunAsync(window, new[] { "Stay or go?", "Stay", "Go" }, Presses.ContextWith(window, DialogueWindowKind.Question), CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void ThenTheConfirmSoundPlays()
        {
            Assert.AreEqual(1, m_Sfx.Played.Count);
            Assert.AreEqual(FakeSystemSoundConfig.CONFIRM, m_Sfx.Played[0]);
        }
    }
}
