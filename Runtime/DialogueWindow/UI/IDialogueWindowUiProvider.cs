using UnityEngine.UIElements;

namespace RPGFramework.Core.DialogueWindow.UI
{
    public interface IDialogueWindowUiProvider
    {
        VisualTreeAsset Get<T>() where T : IDialogueWindowUI;
        float           GetTextSpeed   { get; }
        float           GetWindowSpeed { get; }
    }
}