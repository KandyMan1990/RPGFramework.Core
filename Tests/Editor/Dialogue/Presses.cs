using System.Threading.Tasks;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>Windows opened in an input context, and the player pressing confirm on them.</summary>
    internal static class Presses
    {
        internal static DialogueInputContext ContextWith(IDialogueWindow window, DialogueWindowKind kind)
        {
            DialogueInputContext context = new DialogueInputContext();
            context.Open(window, kind);

            return context;
        }

        internal static FakeDialogueWindow NewWindow()
        {
            return new FakeDialogueWindow(Task.CompletedTask);
        }

        /// <returns>Whether the windows kept the press from what is under them.</returns>
        internal static bool Press(DialogueInputContext context)
        {
            return ((IInputContext)context).Handle(ControlSlot.Primary);
        }
    }
}
