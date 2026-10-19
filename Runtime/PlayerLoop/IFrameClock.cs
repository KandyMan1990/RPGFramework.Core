using UnityEngine;

namespace RPGFramework.Core.PlayerLoop
{
    /// <summary>How long the last frame took, for what counts time as it passes.</summary>
    internal interface IFrameClock
    {
        /// <summary>Seconds since the last frame, unaffected by the time scale.</summary>
        float UnscaledDeltaTime { get; }
    }

    internal sealed class UnityFrameClock : IFrameClock
    {
        float IFrameClock.UnscaledDeltaTime => Time.unscaledDeltaTime;
    }
}
