namespace RPGFramework.Core.Tests.PlayerLoop
{
    internal sealed class CountingUpdatable : IUpdatable
    {
        internal int Ticks { get; private set; }

        void IUpdatable.Update()
        {
            Ticks++;
        }
    }
}
