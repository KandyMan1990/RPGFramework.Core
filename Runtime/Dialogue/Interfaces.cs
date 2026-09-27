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

        /// <summary>
        /// How fast the text types, as the player's message speed setting: 0 is half the authored speed, 0.5 the
        /// authored speed and 1 double it. Each step of the setting scales the speed by the same factor.
        /// </summary>
        void SetMessageSpeed(float                  messageSpeed);

        void SetRect(RectInt                        rect);
        void SetStyle(DialogueWindowStyle           style);
    }
}