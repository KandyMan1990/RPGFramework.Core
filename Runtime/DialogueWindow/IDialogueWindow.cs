using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.DialogueWindow
{
    public interface IDialogueWindow
    {
        Task AnimateWindowClosedAsync();
        Task AnimateWindowOpenAsync();
        void Destroy();
        void Init(VisualElement container, ulong id);
        Task RunAsync(string    dialogue, DialogueInputContext inputContext);
        void SetRect(RectInt    rect);
    }
}