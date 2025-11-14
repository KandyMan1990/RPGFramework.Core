using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public interface IUpdatable
    {
        void Update();
    }

    public interface IEntryPoint
    {
        void Bind<TInterface, TConcrete>() where TConcrete : TInterface;
        void BindSingleton<TInterface, TConcrete>() where TConcrete : TInterface;
        void BindSingletonFromInstance<TInterface>(TInterface instance);
        Task StartGameAsync();
    }

    public interface IModuleArgs
    {

    }

    public interface IModule
    {
        Task OnEnterAsync(IModuleArgs args);
        Task OnExitAsync();
    }

    public interface ICoreFieldModule
    {
        void ResetModule<TInterface, TConcrete>() where TConcrete : TInterface;
    }

    public interface ICoreMenuModule
    {
        void ResetModule<TInterface, TConcrete>() where TConcrete : TInterface;
    }
    
    public interface IMenuModule : IModule
    {

    }

    public interface IMenuModuleArgs : IModuleArgs
    {

    }

    public struct MenuModuleArgs : IMenuModuleArgs
    {

    }

    public interface IMenu
    {

    }
    
    public interface IFieldModule : IModule
    {

    }

    public interface IFieldModuleArgs : IModuleArgs
    {

    }

    public struct FieldModuleArgs : IFieldModuleArgs
    {

    }

    public interface IField
    {

    }
}