using System.Threading.Tasks;
using RPGFramework.Core.Input;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// Covers and uncovers the screen, drawing on an overlay above every module's UI that lasts across module changes.
    /// Core covers and reveals around every change between modules; a module asks for its own steps, such as a field
    /// jump or a menu change. A cover asked for while the screen is covered, or a reveal while it is clear, does
    /// nothing; one asked for during another takes over from wherever the screen is. Routed input is held back from a
    /// cover starting until the screen is clear again. On unscaled time, so it runs while the game is frozen.
    /// </summary>
    public interface ITransitionService
    {
        /// <summary>Covers the screen with <paramref name="transition" />, or the game's default cover.</summary>
        Task CoverAsync(TransitionAsset transition = null);

        /// <summary>Uncovers the screen with <paramref name="transition" />, or the game's default reveal.</summary>
        Task RevealAsync(TransitionAsset transition = null);
    }

    // The screen starts covered, so nothing shows until the first module is revealed.
    internal sealed class TransitionService : ITransitionService
    {
        private readonly ITransitionConfig m_Config;
        private readonly IInputLock        m_InputLock;
        private readonly VisualElement     m_Host;

        private TransitionPlayback m_Playback;
        private float              m_Covered;
        private int                m_Run;

        public TransitionService(ITransitionConfig config, IInputLock inputLock)
        {
            m_Config    = config;
            m_InputLock = inputLock;
            m_Host      = CreateOverlay(config.PanelSettings);
            m_Playback  = config.DefaultCover.Begin(m_Host);
            m_Covered   = 1f;
            m_Run       = 0;

            m_Playback.Show(1f);
            m_InputLock.Lock();
        }

        async Task ITransitionService.CoverAsync(TransitionAsset transition)
        {
            if (m_Covered >= 1f)
            {
                return;
            }

            if (m_Covered <= 0f)
            {
                m_InputLock.Lock();
                m_Host.pickingMode = PickingMode.Position;
            }

            TransitionAsset cover = transition != null ? transition : m_Config.DefaultCover;
            int             run   = ++m_Run;

            Begin(cover);

            await AnimateAsync(run, cover, 1f);
        }

        async Task ITransitionService.RevealAsync(TransitionAsset transition)
        {
            if (m_Covered <= 0f)
            {
                return;
            }

            TransitionAsset reveal = transition != null ? transition : m_Config.DefaultReveal;
            int             run    = ++m_Run;

            Begin(reveal);

            bool finished = await AnimateAsync(run, reveal, 0f);

            if (!finished)
            {
                return;
            }

            m_Playback.End();
            m_Playback = null;

            m_Host.pickingMode = PickingMode.Ignore;
            m_InputLock.Unlock();
        }

        // The new transition is drawn at the old one's amount before the old one goes, so the swap never shows what
        // the screen was hiding.
        private void Begin(TransitionAsset transition)
        {
            TransitionPlayback previous = m_Playback;

            m_Playback = transition.Begin(m_Host);
            m_Playback.Show(m_Covered);

            previous?.End();
        }

        // False if another cover or reveal took over before it finished.
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

            return true;
        }

        private void SetCovered(float covered)
        {
            m_Covered = covered;

            m_Playback.Show(covered);
        }

        private static VisualElement CreateOverlay(PanelSettings panelSettings)
        {
            GameObject overlay = new GameObject("Transitions");
            Object.DontDestroyOnLoad(overlay);

            UIDocument document = overlay.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;

            VisualElement host = document.rootVisualElement;

            host.style.position = Position.Absolute;
            host.style.left     = 0f;
            host.style.top      = 0f;
            host.style.right    = 0f;
            host.style.bottom   = 0f;
            host.pickingMode    = PickingMode.Position;

            return host;
        }
    }
}