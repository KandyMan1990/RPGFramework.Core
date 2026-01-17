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
        Task LoadModuleAsync<T>(IModuleArgs args) where T : IModule;
        Task LoadModuleAsync(Type           type, IModuleArgs args);
        void ResetModule<TInterface, TConcrete>() where TConcrete : TInterface where TInterface : IModule;
        Task ResumeModuleAsync();
    }
    
    public interface IModuleResumeMap
    {
        Type        GetModuleType(byte moduleId);
        IModuleArgs CreateArgs(byte    moduleId, int arg0, int arg1, int arg2, int arg3);
    }
}