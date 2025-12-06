using System.Threading.Tasks;
using RPGFramework.Core.SharedTypes;

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