using UnityEngine;

namespace RPGFramework.Core.Rendering
{
    /// <summary>How long the screen fades take, and the materials each kind of fade draws with.</summary>
    public interface IScreenFadeServiceConfig
    {
        /// <summary>Seconds a fade out takes.</summary>
        float FadeOutTime { get; }

        /// <summary>Seconds a fade in takes.</summary>
        float FadeInTime { get; }

        /// <summary>The plain fade to black most changes use.</summary>
        Material SimpleFadeMaterial { get; }

        /// <summary>The transition the screen goes out with as a battle starts.</summary>
        Material BattleStartMaterial { get; }

        /// <summary>The transition the battle comes in with.</summary>
        Material BattleRevealMaterial { get; }
    }

    [CreateAssetMenu(menuName = "RPG Framework/Rendering/Screen Fade Config", fileName = "ScreenFadeConfig")]
    public class ScreenFadeServiceConfig : ScriptableObject, IScreenFadeServiceConfig
    {
        [SerializeField]
        private float m_FadeOutTime = 1.0f;

        [SerializeField]
        private float m_FadeInTime = 1.0f;

        [SerializeField]
        private Material m_SimpleFadeMaterial;

        [SerializeField]
        private Material m_BattleStartMaterial;

        [SerializeField]
        private Material m_BattleRevealMaterial;

        float IScreenFadeServiceConfig.FadeOutTime => m_FadeOutTime;

        float IScreenFadeServiceConfig.FadeInTime => m_FadeInTime;

        Material IScreenFadeServiceConfig.SimpleFadeMaterial => m_SimpleFadeMaterial;

        Material IScreenFadeServiceConfig.BattleStartMaterial => m_BattleStartMaterial;

        Material IScreenFadeServiceConfig.BattleRevealMaterial => m_BattleRevealMaterial;
    }
}