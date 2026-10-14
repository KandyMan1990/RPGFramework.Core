using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPGFramework.Core.Input;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGFramework.Core.Rendering
{
    /// <summary>
    /// Covers and uncovers the screen with a full-screen pass, plainly or with a battle's transition. A fade starts
    /// from where the screen is, so one asked for while the screen is already there does nothing. On unscaled time, so
    /// it runs while the game is frozen. Input routed through the <see cref="IInputRouter" /> is held back while a
    /// fade runs.
    /// </summary>
    public interface IScreenFadeService
    {
        /// <summary>
        /// Covers the screen over the configured time, or at once when <paramref name="immediate" />.
        /// </summary>
        Task FadeOutAsync(bool immediate = false);

        /// <summary>
        /// Uncovers the screen over the configured time, or at once when <paramref name="immediate" />.
        /// </summary>
        Task FadeInAsync(bool immediate = false);

        /// <summary>Fades from now on use the plain fade to black.</summary>
        void SetFadeToSimple();

        /// <summary>Fades from now on use the battle-start transition.</summary>
        void SetFadeToBattleStart();

        /// <summary>Fades from now on use the battle's reveal.</summary>
        void SetFadeToBattleReveal();
    }

    internal class ScreenFadeService : IScreenFadeService
    {
        private const string RENDERER_FEATURE_NAME = "RPGFrameworkFade";

        private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

        private readonly IScreenFadeServiceConfig      m_ScreenFadeServiceConfig;
        private readonly FullScreenPassRendererFeature m_RendererFeature;
        private readonly IInputLock                    m_InputLock;

        // How far faded the screen is, 0 clear to 1 out, whichever material draws it.
        private float m_Amount;

        internal ScreenFadeService(IScreenFadeServiceConfig config, IRendererDataProvider rendererDataProvider, IInputLock inputLock)
        {
            m_ScreenFadeServiceConfig = config;
            m_InputLock               = inputLock;

            UniversalRendererData rendererData = rendererDataProvider.Get();
            ScriptableRendererFeature rendererFeature =
                rendererData.rendererFeatures.FirstOrDefault(r => r.name.Equals(RENDERER_FEATURE_NAME));

            if (rendererFeature == null)
            {
                throw new KeyNotFoundException(RENDERER_FEATURE_NAME);
            }

            m_RendererFeature = (FullScreenPassRendererFeature)rendererFeature;
            m_Amount          = m_RendererFeature.passMaterial.GetFloat(DissolveAmount);
        }

        Task IScreenFadeService.FadeOutAsync(bool immediate)
        {
            return FadeAsync(1f, m_ScreenFadeServiceConfig.FadeOutTime, immediate);
        }

        Task IScreenFadeService.FadeInAsync(bool immediate)
        {
            return FadeAsync(0f, m_ScreenFadeServiceConfig.FadeInTime, immediate);
        }

        void IScreenFadeService.SetFadeToSimple()
        {
            SetMaterial(m_ScreenFadeServiceConfig.SimpleFadeMaterial);
        }

        void IScreenFadeService.SetFadeToBattleStart()
        {
            SetMaterial(m_ScreenFadeServiceConfig.BattleStartMaterial);
        }

        void IScreenFadeService.SetFadeToBattleReveal()
        {
            SetMaterial(m_ScreenFadeServiceConfig.BattleRevealMaterial);
        }

        // The new material takes the screen as it is, so a swap never shows what a fade had hidden.
        private void SetMaterial(Material material)
        {
            m_RendererFeature.passMaterial = material;

            SetAmount(m_Amount);
        }

        // From wherever the screen is: a screen already there stays put, rather than jumping to the far end first.
        private async Task FadeAsync(float to, float duration, bool immediate)
        {
            if (immediate || m_Amount == to)
            {
                SetAmount(to);
                return;
            }

            float from    = m_Amount;
            float elapsed = 0f;

            m_InputLock.Lock();

            try
            {
                while (elapsed < duration)
                {
                    elapsed = math.min(elapsed + Time.unscaledDeltaTime, duration);
                    float t     = elapsed / duration;
                    float value = math.lerp(from, to, t);

                    SetAmount(value);

                    await Awaitable.NextFrameAsync();
                }

                SetAmount(to);
            }
            finally
            {
                m_InputLock.Unlock();
            }
        }

        private void SetAmount(float amount)
        {
            m_Amount = amount;

            m_RendererFeature.passMaterial.SetFloat(DissolveAmount, amount);
        }
    }
}