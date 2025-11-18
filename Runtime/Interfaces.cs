using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public interface IUpdatable
    {
        void Update();
    }

    public interface IEntryPoint
    {
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
}