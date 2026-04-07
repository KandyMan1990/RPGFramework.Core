using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue.UI
{
    [CreateAssetMenu(menuName = "RPG Framework/Dialogue/Dialogue Window UI Provider", fileName = "Dialogue Window UI Provider")]
    public class DialogueWindowUiProvider : ScriptableObject, IDialogueWindowUiProvider
    {
        [SerializeField] private float           m_GetTextSpeed;
        [SerializeField] private float           m_GetWindowSpeed;
        [SerializeField] private VisualTreeAsset m_DialogueWindow;

        VisualTreeAsset IDialogueWindowUiProvider.Get<T>()
        {
            return m_DialogueWindow;
        }

        /*
        Example of how to implement multiple windows
        private void OnValidate()
        {
            m_Assets = new Dictionary<Type, VisualTreeAsset>
                       {
                               { typeof(MyDialogueWindowUI), m_MyDialogueWindow },
                               { typeof(MyDifferentDialogueWindowUI), m_MyDifferentDialogueWindow },
                       };
        }

        VisualTreeAsset IDialogueWindowUiProvider.Get<T>()
        {
            m_Assets.TryGetValue(typeof(T), out VisualTreeAsset asset);

            return asset;
        }
        */

        float IDialogueWindowUiProvider.GetTextSpeed   => m_GetTextSpeed;
        float IDialogueWindowUiProvider.GetWindowSpeed => m_GetWindowSpeed;
    }
}