using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public class NullModule : IModule
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