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
        void SetMessageSpeed(float                  messageSpeed);
        void SetRect(RectInt                        rect);
        void SetStyle(DialogueWindowStyle           style);
        void SetText(DialoguePage                   dialoguePage);
        void SkipToAnimationEnd();
    }

    public interface IDialogueWindowUIProvider
    {
        VisualTreeAsset    Get<T>() where T : IDialogueWindowUI;
        float              TextSpeed   { get; }
        float              WindowSpeed { get; }
        DialogueTextStyles TextStyles  { get; }
    }
}