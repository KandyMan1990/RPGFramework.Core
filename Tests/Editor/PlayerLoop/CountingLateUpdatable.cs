namespace RPGFramework.Core.Tests.PlayerLoop
{
    internal sealed class CountingLateUpdatable : ILateUpdatable
    {
        internal int Ticks { get; private set; }

        void ILateUpdatable.LateUpdate()
        {
            Ticks++;
        }
    }
}
