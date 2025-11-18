using System;
using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public partial class CoreModule : ICoreMenuModule
    {
        Task ICoreMenuModule.LoadFieldModulesAsync(IFieldModuleArgs args)
        {
            return LoadModuleAsync<IFieldModule>(args);
        }

        void ICoreMenuModule.ResetModule<TConcrete>()
        {
            m_CoreModuleDIContainer.BindSingleton<IMenuModule, TConcrete>();
        }

        T ICoreMenuModule.GetInstance<T>()
        {
            return m_CoreModuleDIContainer.Resolve<T>();
        }

        object ICoreMenuModule.GetInstance(Type type)
        {
            return m_CoreModuleDIContainer.Resolve(type);
        }
    }
}