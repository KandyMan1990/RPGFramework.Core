using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGFramework.Core.Rendering
{
    public interface IScreenFadeService
    {
        Task FadeOutAsync(bool immediate = false);
        Task FadeInAsync(bool  immediate = false);
        void SetFadeToSimple();
        void SetFadeToBattleStart();
        void SetFadeToBattleReveal();
    }

    internal class ScreenFadeService : IScreenFadeService
    {
        private const string RENDERER_FEATURE_NAME = "RPGFrameworkFade";

        private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

        private readonly IScreenFadeServiceConfig      m_ScreenFadeServiceConfig;
        private readonly FullScreenPassRendererFeature m_RendererFeature;

        internal ScreenFadeService(IScreenFadeServiceConfig config, IRendererDataProvider rendererDataProvider)
        {
            m_ScreenFadeServiceConfig = config;

            UniversalRendererData rendererData = rendererDataProvider.Get();
            ScriptableRendererFeature rendererFeature =
                rendererData.rendererFeatures.FirstOrDefault(r => r.name.Equals(RENDERER_FEATURE_NAME));

            if (rendererFeature == null)
            {
                throw new KeyNotFoundException(RENDERER_FEATURE_NAME);
            }

            m_RendererFeature = (FullScreenPassRendererFeature)rendererFeature;
        }

        Task IScreenFadeService.FadeOutAsync(bool immediate)
        {
            return FadeAsync(0f, 1f, m_ScreenFadeServiceConfig.FadeOutTime, immediate);
        }

        Task IScreenFadeService.FadeInAsync(bool immediate)
        {
            return FadeAsync(1f, 0f, m_ScreenFadeServiceConfig.FadeInTime, immediate);
        }

        void IScreenFadeService.SetFadeToSimple()
        {
            m_RendererFeature.passMaterial = m_ScreenFadeServiceConfig.SimpleFadeMaterial;
        }

        void IScreenFadeService.SetFadeToBattleStart()
        {
            m_RendererFeature.passMaterial = m_ScreenFadeServiceConfig.BattleStartMaterial;
        }

        void IScreenFadeService.SetFadeToBattleReveal()
        {
            m_RendererFeature.passMaterial = m_ScreenFadeServiceConfig.BattleRevealMaterial;
        }

        private async Task FadeAsync(float from, float to, float duration, bool immediate)
        {
            if (immediate)
            {
                m_RendererFeature.passMaterial.SetFloat(DissolveAmount, to);
                return;
            }

            m_RendererFeature.passMaterial.SetFloat(DissolveAmount, from);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed = math.min(elapsed + Time.deltaTime, duration);
                float t     = elapsed / duration;
                float value = math.lerp(from, to, t);

                m_RendererFeature.passMaterial.SetFloat(DissolveAmount, value);

                await Awaitable.NextFrameAsync();
            }

            m_RendererFeature.passMaterial.SetFloat(DissolveAmount, to);
        }
    }
}