using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue
{
    public interface IDialogueFlow
    {
        Task RunAsync(IDialogueWindowUI uiInstance, string[] dialogues, DialogueInputContext inputContext, CancellationToken close);
    }

    public interface IDialogueWindow
    {
        Task AnimateWindowClosedAsync();
        Task AnimateWindowOpenAsync();
        void Destroy();
        byte GetSelectedChoice();
        void Init(VisualElement                     container);
        Task RunAsync(IDialogueFlow                 dialogueFlow, string[] dialogues, DialogueInputContext inputContext, CancellationToken close);
        void SetMessageVariables(IReadOnlyList<int> variables);
        void SetRect(RectInt                        rect);
        void SetStyle(DialogueWindowStyle           style);
    }
}