using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input
{
    internal sealed class FakeGamepadMotors : IGamepadMotors
    {
        internal float Low     { get; private set; }
        internal float High    { get; private set; }
        internal int   Stopped { get; private set; }

        void IGamepadMotors.Run(float low, float high)
        {
            Low  = low;
            High = high;
        }

        void IGamepadMotors.Stop()
        {
            Stopped++;
        }
    }
}
