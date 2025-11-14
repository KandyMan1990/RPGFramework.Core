using System.Threading.Tasks;

namespace RPGFramework.Core
{
    internal class NullModule : IModule
    {
        Task IModule.OnEnterAsync(IModuleArgs args)
        {
            return Task.CompletedTask;
        }

        Task IModule.OnExitAsync()
        {
            return Task.CompletedTask;
        }
    }
}