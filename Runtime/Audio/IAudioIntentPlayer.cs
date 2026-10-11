using System.Threading.Tasks;

namespace RPGFramework.Core.Audio
{
    /// <summary>
    /// Plays what a module says happened — a confirm in the menu, a cancel — as whatever sound the game gives it, so
    /// Core and the modules never name a sound. Bound by the game.
    /// </summary>
    public interface IAudioIntentPlayer
    {
        /// <summary>
        /// Plays the game's sound for what happened. The task completes once the sound has played out — at once for
        /// an intent the game gives no sound, or a sound that loops — so a caller can wait for it, as quitting does. A
        /// sound stopped before its end may never complete it, so wait only on one nothing else stops.
        /// </summary>
        Task PlayAsync(AudioIntent intent, AudioContext context);
    }
}