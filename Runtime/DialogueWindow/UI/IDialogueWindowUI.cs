using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.DialogueWindow.UI
{
    public interface IDialogueWindowUI
    {
        Task AnimateWindowClosedAsync();
        Task AnimateWindowOpenAsync();
        void Destroy();
        void Init(VisualElement container, ulong id);
        Task RunAsync();
        void SetRect(RectInt      rect);
        void SetText(DialoguePage dialoguePage);
        void SkipToAnimationEnd();
    }
}