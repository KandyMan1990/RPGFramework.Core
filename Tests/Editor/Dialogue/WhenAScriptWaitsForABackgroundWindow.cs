using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>
    /// A background window stays up through presses until its script waits for it to close; from then, a press closes
    /// it once it has finished.
    /// </summary>
    internal sealed class WhenAScriptWaitsForABackgroundWindow
    {
        private FakeDialogueWindow   m_Window;
        private DialogueInputContext m_Context;

        [SetUp]
        public void SetUp()
        {
            m_Window  = Presses.NewWindow();
            m_Context = Presses.ContextWith(m_Window, DialogueWindowKind.Background);
        }

        [Test]
        public void ThenBeforeItWaitsAPressLeavesItUp()
        {
            Task pressToClose = m_Context.WaitForPressToCloseAsync(m_Window);

            Presses.Press(m_Context);

            Assert.IsFalse(pressToClose.IsCompleted);
        }

        [Test]
        public void ThenOnceItWaitsAPressClosesIt()
        {
            Task pressToClose = m_Context.WaitForPressToCloseAsync(m_Window);

            m_Context.LetPressClose(m_Window);
            Presses.Press(m_Context);

            Assert.IsTrue(pressToClose.IsCompleted);
        }

        [Test]
        public void ThenThePressThatTurnedAnEarlierPageDoesNotCloseIt()
        {
            m_Context.LetPressClose(m_Window);
            Presses.Press(m_Context);

            Task pressToClose = m_Context.WaitForPressToCloseAsync(m_Window);

            Assert.IsFalse(pressToClose.IsCompleted);
        }
    }
}
