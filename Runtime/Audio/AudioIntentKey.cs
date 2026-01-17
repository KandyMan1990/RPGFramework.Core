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

        public bool Equals(AudioIntentKey other) => Intent == other.Intent && Context == other.Context;

        public override bool Equals(object obj) => obj is AudioIntentKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Intent, (int)Context);
    }
}