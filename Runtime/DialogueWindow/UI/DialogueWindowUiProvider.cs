using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.DialogueWindow.UI
{
    [CreateAssetMenu(menuName = "RPG Framework/Dialogue/Dialogue Window UI Provider", fileName = "Dialogue Window UI Provider")]
    public class DialogueWindowUiProvider : ScriptableObject, IDialogueWindowUiProvider
    {
        [SerializeField] private float           m_GetTextSpeed;
        [SerializeField] private float           m_GetWindowSpeed;
        [SerializeField] private VisualTreeAsset m_DialogueWindowWithChoice;
        [SerializeField] private VisualTreeAsset m_DialogueWindowWithText;

        private Dictionary<Type, VisualTreeAsset> m_Assets;

        private void OnValidate()
        {
            m_Assets = new Dictionary<Type, VisualTreeAsset>
                       {
                               { typeof(IDialogueWindowWithChoiceUI), m_DialogueWindowWithChoice },
                               { typeof(IDialogueWindowWithTextUI), m_DialogueWindowWithText },
                       };
        }

        VisualTreeAsset IDialogueWindowUiProvider.Get<T>()
        {
            m_Assets.TryGetValue(typeof(T), out VisualTreeAsset asset);

            return asset;
        }
        float IDialogueWindowUiProvider.GetTextSpeed   => m_GetTextSpeed;
        float IDialogueWindowUiProvider.GetWindowSpeed => m_GetWindowSpeed;
    }
}