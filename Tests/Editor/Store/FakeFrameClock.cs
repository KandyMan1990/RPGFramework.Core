using RPGFramework.Core.PlayerLoop;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>A clock whose frames take as long as the test says.</summary>
    internal sealed class FakeFrameClock : IFrameClock
    {
        internal float Seconds { get; set; }

        float IFrameClock.UnscaledDeltaTime => Seconds;
    }
}
