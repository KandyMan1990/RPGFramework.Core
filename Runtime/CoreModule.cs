using System;
using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public class CoreModule : IEntryPoint, ICoreFieldModule, ICoreMenuModule
    {
        private readonly DIContainer m_GlobalContainer;

        private IModule m_CurrentModule;

        private CoreModule()
        {
            m_CurrentModule   = new NullModule();
            m_GlobalContainer = new DIContainer();
            IEntryPoint entryPoint = this;

            entryPoint.BindSingletonFromInstance<ICoreFieldModule, CoreModule>(this);
            entryPoint.BindSingletonFromInstance<ICoreMenuModule, CoreModule>(this);
        }

        public static IEntryPoint Create()
        {
            return new CoreModule();
        }

        void IEntryPoint.BindTransient<TInterface, TConcrete>()
        {
            m_GlobalContainer.BindTransient<TInterface, TConcrete>();
        }

        void IEntryPoint.BindSingleton<TInterface, TConcrete>()
        {
            m_GlobalContainer.BindSingleton<TInterface, TConcrete>();
        }

        void IEntryPoint.BindSingletonFromInstance<TInterface, TConcrete>(TConcrete instance)
        {
            m_GlobalContainer.BindSingletonFromInstance<TInterface, TConcrete>(instance);
        }

        Task IEntryPoint.StartGameAsync()
        {
            MenuModuleArgs args = new MenuModuleArgs
                                  {
                                          MenuType = typeof(IBeginMenu)
                                  };

            return LoadModuleAsync<IMenuModule>(args);
        }

        void ICoreFieldModule.ResetModule<TConcrete>()
        {
            m_GlobalContainer.BindSingleton<IFieldModule, TConcrete>();
        }

        T ICoreMenuModule.GetInstance<T>()
        {
            return m_GlobalContainer.Resolve<T>();
        }

        object ICoreMenuModule.GetInstance(Type type)
        {
            return m_GlobalContainer.Resolve(type);
        }

        void ICoreMenuModule.ResetModule<TConcrete>()
        {
            m_GlobalContainer.BindSingleton<IMenuModule, TConcrete>();
        }

        private async Task LoadModuleAsync<T>(IModuleArgs args) where T : IModule
        {
            await m_CurrentModule.OnExitAsync();

            m_CurrentModule = m_GlobalContainer.Resolve<T>();

            await m_CurrentModule.OnEnterAsync(args);
        }
    }
}