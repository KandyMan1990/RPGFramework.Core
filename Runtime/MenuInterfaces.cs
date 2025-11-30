using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace RPGFramework.Core
{
    public interface ICoreMenuModule
    {
        Task   LoadFieldModulesAsync(IFieldModuleArgs args);
        void   ResetModule<TConcrete>() where TConcrete : IMenuModule;
        T      GetInstance<T>();
        object GetInstance(Type type);
    }

    public interface IMenuModule : IModule
    {
        Task PushMenu(IMenuModuleArgs menuModuleArgs);
        Task PopMenu();
        void PlaySfx(int id);
    }

    public interface IMenuModuleArgs : IModuleArgs
    {
        Type MenuType { get; }
    }

    public readonly struct MenuModuleArgs : IMenuModuleArgs
    {
        public Type MenuType { get; }

        public MenuModuleArgs(Type menuType)
        {
            MenuType = menuType;
        }
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
        event Action<int> OnPlayAudio;
        Task              OnEnterAsync(VisualElement rootContainer);
        Task              OnSuspendAsync();
        Task              OnResumeAsync();
        Task              OnExitAsync();
    }

    public interface IBeginMenu : IMenu
    {

    }

    public interface IBeginMenuUI : IMenuUI
    {
        event Action OnNewGame;
        event Action OnLoadGame;
        event Action OnSettings;
        event Action OnQuit;
    }
}