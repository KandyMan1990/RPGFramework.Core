using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>Three windows up, the newest gone: the focus passes to the newest still open.</summary>
    internal sealed class WhenTheFocusedWindowCloses
    {
        private Task m_FirstFocused;
        private Task m_SecondFocused;

        [SetUp]
        public void SetUp()
        {
            FakeDialogueWindow first  = Presses.NewWindow();
            FakeDialogueWindow second = Presses.NewWindow();
            FakeDialogueWindow third  = Presses.NewWindow();

            DialogueInputContext context = Presses.ContextWith(first, DialogueWindowKind.Background);
            context.Open(second, DialogueWindowKind.Background);
            context.Open(third, DialogueWindowKind.Message);
            context.Close(third);

            m_FirstFocused  = context.WaitForFocusedPressAsync(first);
            m_SecondFocused = context.WaitForFocusedPressAsync(second);

            Presses.Press(context);
        }

        [Test]
        public void ThenTheNewestStillOpenHasIt()
        {
            Assert.IsTrue(m_SecondFocused.IsCompleted);
        }

        [Test]
        public void ThenAnOlderOneDoesNot()
        {
            Assert.IsFalse(m_FirstFocused.IsCompleted);
        }
    }
}
