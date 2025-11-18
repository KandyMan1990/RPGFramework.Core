using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public partial class CoreModule : ICoreFieldModule
    {
        Task ICoreFieldModule.LoadMenuModuleAsync(IMenuModuleArgs args)
        {
            return LoadModuleAsync<IMenuModule>(args);
        }

        void ICoreFieldModule.ResetModule<TConcrete>()
        {
            m_CoreModuleDIContainer.BindSingleton<IFieldModule, TConcrete>();
        }
    }
}