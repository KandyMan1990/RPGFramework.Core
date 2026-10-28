using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>A message shown over one already up: the newest takes the focus.</summary>
    internal sealed class WhenANewerWindowOpens
    {
        private Task m_OlderFocused;
        private Task m_NewerFocused;
        private Task m_AnyWindow;

        [SetUp]
        public void SetUp()
        {
            FakeDialogueWindow older = Presses.NewWindow();
            FakeDialogueWindow newer = Presses.NewWindow();

            DialogueInputContext context = Presses.ContextWith(older, DialogueWindowKind.Message);
            context.Open(newer, DialogueWindowKind.Message);

            m_OlderFocused = context.WaitForFocusedPressAsync(older);
            m_NewerFocused = context.WaitForFocusedPressAsync(newer);
            m_AnyWindow    = context.WaitForPressAsync();

            Presses.Press(context);
        }

        [Test]
        public void ThenThePressTurnsTheNewersPage()
        {
            Assert.IsTrue(m_NewerFocused.IsCompleted);
        }

        [Test]
        public void ThenItLeavesTheOldersPage()
        {
            Assert.IsFalse(m_OlderFocused.IsCompleted);
        }

        [Test]
        public void ThenItStillClosesAWindowThatHasFinished()
        {
            Assert.IsTrue(m_AnyWindow.IsCompleted);
        }
    }
}
