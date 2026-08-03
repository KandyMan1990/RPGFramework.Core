using UnityEngine;

namespace RPGFramework.Core.Rendering
{
    public interface IScreenFadeServiceConfig
    {
        float    FadeOutTime          { get; }
        float    FadeInTime           { get; }
        Material SimpleFadeMaterial   { get; }
        Material BattleStartMaterial  { get; }
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