using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input
{
    /// <summary>Counts the presses it is offered, taking each, and takes movement or lets it through as it is made.</summary>
    internal sealed class RecordingInputContext : IInputContext
    {
        private readonly bool m_TakesMovement;

        internal RecordingInputContext(bool takesMovement)
        {
            m_TakesMovement = takesMovement;
        }

        internal int Presses { get; private set; }

        bool IInputContext.TakesMovement => m_TakesMovement;

        bool IInputContext.Handle(ControlSlot slot)
        {
            Presses++;

            return true;
        }
    }
}
