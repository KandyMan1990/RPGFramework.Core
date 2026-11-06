using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Audio;
using UnityEngine.Audio;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>
    /// Keeps the name hash of every sound asked for, in order, and plays none. What it hands back is no sound at all, as
    /// only Audio can make one, so it serves only callers that fire and forget.
    /// </summary>
    internal sealed class FakeSfxPlayer : ISfxPlayer
    {
        private readonly List<ulong> m_Played;

        internal FakeSfxPlayer()
        {
            m_Played = new List<ulong>();
        }

        internal IReadOnlyList<ulong> Played => m_Played;

        Task<ISfxReference> ISfxPlayer.PlayAsync(ulong nameHash)
        {
            m_Played.Add(nameHash);

            return Task.FromResult<ISfxReference>(null);
        }

        void ISfxPlayer.Pause(ISfxReference sfxReference)
        {
        }

        void ISfxPlayer.PauseAll()
        {
        }

        void ISfxPlayer.Resume(ISfxReference sfxReference)
        {
        }

        void ISfxPlayer.ResumeAll()
        {
        }

        void ISfxPlayer.Stop(ISfxReference sfxReference)
        {
        }

        void ISfxPlayer.StopAll()
        {
        }

        void ISfxPlayer.SetSfxAssetProvider(ISfxAssetProvider provider)
        {
        }

        void ISfxPlayer.SetStemMixerGroups(AudioMixerGroup[] groups)
        {
        }

        float ISfxPlayer.GetVolume() => 1f;

        void ISfxPlayer.SetVolume(float percent)
        {
        }

        Task ISfxPlayer.PreloadAsync(IReadOnlyList<ulong> nameHashes) => Task.CompletedTask;

        void ISfxPlayer.Unload(IReadOnlyList<ulong> nameHashes)
        {
        }

        void System.IDisposable.Dispose()
        {
        }
    }
}
