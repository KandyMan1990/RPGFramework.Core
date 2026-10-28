using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>A message shown while a question waits for its answer: the question keeps the focus.</summary>
    internal sealed class WhenAQuestionIsAsked
    {
        private Task m_QuestionFocused;
        private Task m_MessageFocused;

        [SetUp]
        public void SetUp()
        {
            FakeDialogueWindow question = Presses.NewWindow();
            FakeDialogueWindow message  = Presses.NewWindow();

            DialogueInputContext context = Presses.ContextWith(question, DialogueWindowKind.Question);
            context.Open(message, DialogueWindowKind.Message);

            m_QuestionFocused = context.WaitForFocusedPressAsync(question);
            m_MessageFocused  = context.WaitForFocusedPressAsync(message);

            Presses.Press(context);
        }

        [Test]
        public void ThenThePressAnswersIt()
        {
            Assert.IsTrue(m_QuestionFocused.IsCompleted);
        }

        [Test]
        public void ThenANewerWindowDoesNotTakeTheFocus()
        {
            Assert.IsFalse(m_MessageFocused.IsCompleted);
        }
    }
}
