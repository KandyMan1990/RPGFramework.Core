using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// An element transitions play into, each from wherever the last left it. Core's overlay is one; a module makes its
    /// own for what belongs beneath its UI, as a field's scripts fade the screen under its message windows. A transition
    /// started during another takes over at its amount, drawn before the old one goes so the swap never shows what was
    /// hidden; one that has cleared the element is removed. On unscaled time. It plays no sound: what starts a transition
    /// plays its sound, found through <see cref="FindCover" /> or <see cref="FindReveal" />.
    /// </summary>
    public sealed class TransitionLayer
    {
        private readonly VisualElement       m_Host;
        private readonly ITransitionConfig   m_Config;
        private readonly ITransitionDatabase m_Database;

        private TransitionPlayback m_Playback;
        private float              m_Covered;
        private int                m_Run;

        /// <summary>
        /// Plays into <paramref name="host" />, which transitions fill, finding them in the game's database and the
        /// config's defaults. It starts clear.
        /// </summary>
        public TransitionLayer(VisualElement host, ITransitionConfig config, ITransitionDatabase database)
        {
            m_Host     = host;
            m_Config   = config;
            m_Database = database;
            m_Playback = null;
            m_Covered  = 0f;
            m_Run      = 0;
        }

        /// <summary>How covered it is: 0 clear, 1 covered entirely.</summary>
        public float Covered => m_Covered;

        /// <summary>
        /// Covers it with the transition whose name hashes to <paramref name="transition" />, or the default cover for
        /// <c>0</c>: true once covered, false if another transition took over first.
        /// </summary>
        public Task<bool> CoverAsync(ulong transition)
        {
            Task<bool> covering = PlayAsync(FindCover(transition), 1f, null);

            return covering;
        }

        /// <summary>
        /// Uncovers it with the transition whose name hashes to <paramref name="transition" />, or the default reveal
        /// for <c>0</c>: true once clear, false if another transition took over first.
        /// </summary>
        public Task<bool> RevealAsync(ulong transition)
        {
            Task<bool> revealing = PlayAsync(FindReveal(transition), 0f, null);

            return revealing;
        }

        /// <summary>
        /// Covers it entirely, with the cover <paramref name="transition" /> names, or clears it, at once and without a
        /// sound, stopping any transition playing.
        /// </summary>
        public void Cut(ulong transition, bool covered)
        {
            m_Run++;

            if (!covered)
            {
                End();

                return;
            }

            m_Covered = 1f;

            Begin(FindCover(transition), null);
        }

        /// <summary>The cover <paramref name="transition" /> names, or the default cover for <c>0</c>.</summary>
        public TransitionAsset FindCover(ulong transition)
        {
            TransitionAsset cover = transition == 0 ? m_Config.DefaultCover : m_Database.Get(transition);

            return cover;
        }

        internal Task<bool> PlayAsync(TransitionAsset transition, float to, Texture capturedFrame)
        {
            int run = ++m_Run;

            Begin(transition, capturedFrame);

            Task<bool> playing = AnimateAsync(run, transition, to);

            return playing;
        }

        /// <summary>The reveal <paramref name="transition" /> names, or the default reveal for <c>0</c>.</summary>
        public TransitionAsset FindReveal(ulong transition)
        {
            TransitionAsset reveal = transition == 0 ? m_Config.DefaultReveal : m_Database.Get(transition);

            return reveal;
        }

        private void Begin(TransitionAsset transition, Texture capturedFrame)
        {
            TransitionPlayback previous = m_Playback;

            m_Playback = transition.Begin(new TransitionContext(m_Host, capturedFrame, m_Config));
            m_Playback.Show(m_Covered);

            previous?.End();
        }

        private async Task<bool> AnimateAsync(int run, TransitionAsset transition, float to)
        {
            float from     = m_Covered;
            float duration = transition.Seconds * math.abs(to - from);
            float elapsed  = 0f;

            while (elapsed < duration)
            {
                elapsed = math.min(elapsed + Time.unscaledDeltaTime, duration);

                SetCovered(math.lerp(from, to, transition.Evaluate(elapsed / duration)));

                await Awaitable.NextFrameAsync();

                if (run != m_Run)
                {
                    return false;
                }
            }

            SetCovered(to);

            if (to <= 0f)
            {
                End();
            }

            return true;
        }

        private void SetCovered(float covered)
        {
            m_Covered = covered;

            m_Playback.Show(covered);
        }

        private void End()
        {
            m_Playback?.End();

            m_Playback = null;
            m_Covered  = 0f;
        }
    }
}