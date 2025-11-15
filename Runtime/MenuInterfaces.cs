using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace RPGFramework.Core
{
    public interface IMenuModule : IModule
    {
        Task PushMenu(IMenuModuleArgs menuModuleArgs);
        Task PopMenu();
    }

    public interface IMenuModuleArgs : IModuleArgs
    {
        Type MenuType { get; }
    }

    public struct MenuModuleArgs : IMenuModuleArgs
    {
        public Type MenuType { get; set; }
    }

    public interface IMenuUIProvider
    {
        VisualTreeAsset GetMenuUI<T>() where T : IMenu;
    }

    public interface IMenu
    {
        Task OnEnterAsync(VisualElement rootContainer);
        Task OnSuspendAsync();
        Task OnResumeAsync();
        Task OnExitAsync();
    }

    public interface IMenuUI
    {
        Task OnEnterAsync(VisualElement rootContainer);
        Task OnSuspendAsync();
        Task OnResumeAsync();
        Task OnExitAsync();
    }

    public interface IBeginMenu : IMenu
    {

    }

    public interface IBeginMenuUI : IMenuUI
    {

    }
}