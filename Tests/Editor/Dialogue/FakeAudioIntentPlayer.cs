using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.Audio;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>A sound asked for: what happened, and where.</summary>
    internal readonly struct PlayedIntent
    {
        internal readonly AudioIntent  Intent;
        internal readonly AudioContext Context;

        internal PlayedIntent(AudioIntent intent, AudioContext context)
        {
            Intent  = intent;
            Context = context;
        }
    }

    /// <summary>Keeps every sound asked for, in order, and plays none.</summary>
    internal sealed class FakeAudioIntentPlayer : IAudioIntentPlayer
    {
        private readonly List<PlayedIntent> m_Played;

        internal FakeAudioIntentPlayer()
        {
            m_Played = new List<PlayedIntent>();
        }

        internal IReadOnlyList<PlayedIntent> Played => m_Played;

        Task IAudioIntentPlayer.PlayAsync(AudioIntent intent, AudioContext context)
        {
            m_Played.Add(new PlayedIntent(intent, context));

            return Task.CompletedTask;
        }
    }
}
