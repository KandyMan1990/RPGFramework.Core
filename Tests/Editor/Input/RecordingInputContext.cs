using RPGFramework.Core.Input;
using UnityEngine;

namespace RPGFramework.Core.Tests.Input
{
    internal sealed class RecordingInputContext : IInputContext
    {
        internal int     Presses  { get; private set; }
        internal int     Moves    { get; private set; }
        internal Vector2 LastMove { get; private set; }

        bool IInputContext.Handle(ControlSlot slot)
        {
            Presses++;

            return true;
        }

        bool IInputContext.HandleMove(Vector2 move)
        {
            Moves++;
            LastMove = move;

            return true;
        }
    }
}
