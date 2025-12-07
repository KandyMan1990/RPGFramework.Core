using System;
using System.Threading.Tasks;
using RPGFramework.Core.SharedTypes;

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
        object GetInstance(Type               type);
        Task   LoadModuleAsync<T>(IModuleArgs args) where T : IModule;
        Task   LoadModuleAsync(Type           type, IModuleArgs args);
        void   ResetModule<TInterface, TConcrete>() where TConcrete : TInterface where TInterface : IModule;
    }
}