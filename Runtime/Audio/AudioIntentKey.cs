using System;

namespace RPGFramework.Core.Audio
{
    public readonly struct AudioIntentKey : IEquatable<AudioIntentKey>
    {
        public readonly AudioIntent  Intent;
        public readonly AudioContext Context;

        public AudioIntentKey(AudioIntent intent, AudioContext context)
        {
            Intent  = intent;
            Context = context;
        }

        bool IEquatable<AudioIntentKey>.Equals(AudioIntentKey other) => Matches(other);

        public override bool Equals(object obj) => obj is AudioIntentKey other && Matches(other);

        public override int GetHashCode() => HashCode.Combine((int)Intent, (int)Context);

        private bool Matches(AudioIntentKey other) => Intent == other.Intent && Context == other.Context;
    }
}