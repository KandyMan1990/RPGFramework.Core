namespace RPGFramework.Core.Tests.PlayerLoop
{
    internal sealed class CountingFixedUpdatable : IFixedUpdatable
    {
        internal int Ticks { get; private set; }

        void IFixedUpdatable.FixedUpdate()
        {
            Ticks++;
        }
    }
}
