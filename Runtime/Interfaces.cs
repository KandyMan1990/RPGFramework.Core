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
        void BindTransient<TInterface, TConcrete>() where TConcrete : TInterface;
        void BindSingleton<TInterface, TConcrete>() where TConcrete : TInterface;
        void BindSingletonFromInstance<TInterface, TConcrete>(TConcrete instance) where TConcrete : TInterface;
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
        void ResetModule<TConcrete>() where TConcrete : IFieldModule;
    }

    public interface ICoreMenuModule
    {
        T    GetInstance<T>();
        object    GetInstance(Type type);
        void ResetModule<TConcrete>() where TConcrete : IMenuModule;
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