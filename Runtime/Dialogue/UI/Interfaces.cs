using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue.UI
{
    public interface IDialogueWindowUI
    {
        Task AnimateWindowClosedAsync();
        Task AnimateWindowOpenAsync();
        void Destroy();
        int  GetSelectedChoice();
        void Init(VisualElement container);
        Task RunAsync();
        void SetChoices(ReadOnlySpan<string> choices);
        void SetRect(RectInt                 rect);
        void SetText(DialoguePage            dialoguePage);
        void SkipToAnimationEnd();
    }

    public interface IDialogueWindowUiProvider
    {
        VisualTreeAsset Get<T>() where T : IDialogueWindowUI;
        float           GetTextSpeed   { get; }
        float           GetWindowSpeed { get; }
    }
}