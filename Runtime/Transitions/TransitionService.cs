using System.Threading.Tasks;
using RPGFramework.Audio;
using RPGFramework.Core.Input;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// Covers and uncovers the screen, drawing on an overlay above every module's UI that lasts across module changes.
    /// Core covers and reveals around every change between modules; a module asks for its own steps, such as a field
    /// jump or a menu change. A cover asked for while the screen is covered, or a reveal while it is clear, does
    /// nothing; one asked for during another takes over from wherever the screen is, each playing its sound as it starts.
    /// Routed input is held back from a cover starting until the screen is clear again. On unscaled time, so it runs
    /// while the game is frozen.
    /// </summary>
    public interface ITransitionService
    {
        /// <summary>
        /// Covers the screen with the transition whose name hashes to <paramref name="transition" /> in the game's
        /// <see cref="ITransitionDatabase" />, or with the default cover for <c>0</c>, completing once what it hides may
        /// change: once covered, or at once for a cover over a captured frame, which hides the screen from its first
        /// frame and plays on.
        /// </summary>
        Task CoverAsync(ulong transition = 0);

        /// <summary>
        /// Uncovers the screen with the transition whose name hashes to <paramref name="transition" />, or with the
        /// default reveal for <c>0</c>, once a cover still playing has finished: a cover always plays its full time.
        /// </summary>
        Task RevealAsync(ulong transition = 0);

        /// <summary>
        /// How long the cover <paramref name="transition" /> names takes, <c>0</c> for the default's, for a music fade
        /// timed to it.
        /// </summary>
        float CoverSeconds(ulong transition = 0);
    }

    // The screen starts covered, so nothing shows until the first module is revealed. Black lies beneath the transition
    // while the screen is covered and under a captured frame: a transition drawn by a backdrop filter draws nothing in a
    // frame no camera renders, as when one module's scene has gone and the next's has not yet loaded.
    internal sealed class TransitionService : ITransitionService
    {
        private readonly IInputLock      m_InputLock;
        private readonly VisualElement   m_Host;
        private readonly VisualElement   m_Backdrop;
        private readonly TransitionLayer m_Layer;

        private Task          m_Covering;
        private RenderTexture m_Frame;

        public TransitionService(ITransitionConfig config, ITransitionDatabase database, IInputLock inputLock, ISfxPlayer sfxPlayer)
        {
            m_InputLock = inputLock;
            m_Host      = CreateOverlay(config.PanelSettings);
            m_Backdrop  = CreateBackdrop(m_Host);
            m_Layer     = new TransitionLayer(m_Host, config, database, sfxPlayer);
            m_Covering  = Task.CompletedTask;
            m_Frame     = null;

            m_Layer.Cut(0, true);
            m_InputLock.Lock();
        }

        async Task ITransitionService.RevealAsync(ulong transition)
        {
            await m_Covering;

            if (m_Layer.Covered <= 0f)
            {
                return;
            }

            Task<bool> revealing = m_Layer.RevealAsync(transition);

            ClearBackdrop();

            bool finished = await revealing;

            if (!finished)
            {
                return;
            }

            m_Host.pickingMode = PickingMode.Ignore;
            m_InputLock.Unlock();
        }

        float ITransitionService.CoverSeconds(ulong transition)
        {
            float seconds = m_Layer.FindCover(transition).Seconds;

            return seconds;
        }

        async Task ITransitionService.CoverAsync(ulong transition)
        {
            if (m_Layer.Covered >= 1f)
            {
                return;
            }

            if (m_Layer.Covered <= 0f)
            {
                m_InputLock.Lock();
                m_Host.pickingMode = PickingMode.Position;
            }

            TransitionAsset cover = m_Layer.FindCover(transition);

            ClearBackdrop();

            if (!cover.CapturesFrame)
            {
                bool covered = await m_Layer.PlayAsync(cover, 1f, null);

                if (covered)
                {
                    m_Backdrop.style.display = DisplayStyle.Flex;
                }

                return;
            }

            m_Frame                  = await CaptureAsync();
            m_Backdrop.style.display = DisplayStyle.Flex;

            m_Covering = m_Layer.PlayAsync(cover, 1f, m_Frame);
        }

        private void ClearBackdrop()
        {
            m_Backdrop.style.display = DisplayStyle.None;

            if (m_Frame == null)
            {
                return;
            }

            m_Frame.Release();
            Object.Destroy(m_Frame);

            m_Frame = null;
        }

        // Taken once the frame has drawn, UI included. Where a texture's rows start at the top the capture comes out
        // upside down, so it is turned the right way up before a transition sees it.
        private static async Task<RenderTexture> CaptureAsync()
        {
            await Awaitable.EndOfFrameAsync();

            RenderTexture captured = new RenderTexture(Screen.width, Screen.height, 0);

            ScreenCapture.CaptureScreenshotIntoRenderTexture(captured);

            if (!SystemInfo.graphicsUVStartsAtTop)
            {
                return captured;
            }

            RenderTexture upright = new RenderTexture(Screen.width, Screen.height, 0);

            Graphics.Blit(captured, upright, new Vector2(1f, -1f), new Vector2(0f, 1f));

            captured.Release();
            Object.Destroy(captured);

            return upright;
        }

        private static VisualElement CreateOverlay(PanelSettings panelSettings)
        {
            GameObject overlay = new GameObject("Transitions");
            Object.DontDestroyOnLoad(overlay);

            UIDocument document = overlay.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;

            VisualElement host = document.rootVisualElement;

            Fill(host);
            host.pickingMode = PickingMode.Position;

            return host;
        }

        private static VisualElement CreateBackdrop(VisualElement host)
        {
            VisualElement backdrop = new VisualElement { pickingMode = PickingMode.Ignore };

            Fill(backdrop);
            backdrop.style.backgroundColor = Color.black;

            host.Add(backdrop);

            return backdrop;
        }

        private static void Fill(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.left     = 0f;
            element.style.top      = 0f;
            element.style.right    = 0f;
            element.style.bottom   = 0f;
        }
    }
}