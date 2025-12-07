using UnityEngine.UIElements;

namespace RPGFramework.Core.UI
{
    public static class UIExtensions
    {
        public static void SetEnabledAndVisible(this VisualElement visualElement, bool enabled)
        {
            visualElement.SetEnabled(enabled);
            visualElement.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}