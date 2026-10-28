using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>Cancel reaches the focused question, to move its cursor to its cancel answer.</summary>
    internal sealed class WhenCancelIsPressed
    {
        [Test]
        public void ThenTheFocusedQuestionHearsIt()
        {
            FakeDialogueWindow   question = Presses.NewWindow();
            DialogueInputContext context  = Presses.ContextWith(question, DialogueWindowKind.Question);
            Task                 cancel   = context.WaitForCancelAsync(question);

            Cancel(context);

            Assert.IsTrue(cancel.IsCompleted);
        }

        [Test]
        public void ThenAQuestionKeepsIt()
        {
            DialogueInputContext context = Presses.ContextWith(Presses.NewWindow(), DialogueWindowKind.Question);

            Assert.IsTrue(Cancel(context));
        }

        [Test]
        public void ThenWithOnlyAMessageItCarriesOn()
        {
            DialogueInputContext context = Presses.ContextWith(Presses.NewWindow(), DialogueWindowKind.Message);

            Assert.IsFalse(Cancel(context));
        }

        private static bool Cancel(DialogueInputContext context)
        {
            return ((IInputContext)context).Handle(ControlSlot.Secondary);
        }
    }
}
