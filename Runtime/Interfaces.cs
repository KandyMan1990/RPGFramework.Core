using System;
using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public interface IUpdatable
    {
        void Update();
    }

    public interface IEntryPoint
    {
        Task StartGameAsync<T>(IModuleArgs args) where T : IModule;
    }

    public interface ICoreModule
    {
        T      GetInstance<T>();
        object GetInstance(Type          type);
        Task   LoadModule<T>(IModuleArgs args) where T : IModule;
        void   ResetModule<TInterface, TConcrete>() where TConcrete : TInterface where TInterface : IModule;
    }

    public interface IModuleArgs
    {

    }

    public interface IModule
    {
        Task OnEnterAsync(IModuleArgs args);
        Task OnExitAsync();
    }
}