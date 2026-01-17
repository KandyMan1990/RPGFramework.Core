namespace RPGFramework.Core.Audio
{
    public interface IAudioIntentPlayer
    {
        void Play(AudioIntent intent, AudioContext context);
    }
}