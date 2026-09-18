using System;
using System.Collections.Generic;
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
        byte GetSelectedChoice();
        void Init(VisualElement container);
        Task RunAsync();
        void SetChoices(ReadOnlySpan<string>        choices);
        void SetMessageVariables(IReadOnlyList<int> variables);
        void SetRect(RectInt                        rect);
        void SetStyle(DialogueWindowStyle           style);
        void SetText(DialoguePage                   dialoguePage);
        void SkipToAnimationEnd();
    }

    public interface IDialogueWindowUiProvider
    {
        VisualTreeAsset    Get<T>() where T : IDialogueWindowUI;
        float              GetTextSpeed   { get; }
        float              GetWindowSpeed { get; }
        DialogueTextStyles TextStyles     { get; }
    }
}