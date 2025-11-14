using System;
using System.Threading.Tasks;

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

    public interface IMenu
    {
        Task OnEnterAsync();
        Task OnExitAsync();
    }

    public interface IBeginMenu : IMenu
    {

    }
}