using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RPGFramework.Core
{
    public class CoreModule : IEntryPoint, IDIContainer, ICoreFieldModule, ICoreMenuModule
    {
        private readonly IDIContainer             m_GlobalContainer;
        private readonly Dictionary<Type, string> m_ModuleNames;
        private readonly IDIContainer             m_CoreModuleDIContainer;

        private IModule m_CurrentModule;

        private CoreModule()
        {
            m_GlobalContainer       = new DIContainer();
            m_ModuleNames           = new Dictionary<Type, string>();
            m_CurrentModule         = new NullModule();
            m_CoreModuleDIContainer = this;

            m_CoreModuleDIContainer.BindSingletonFromInstance<ICoreFieldModule, CoreModule>(this);
            m_CoreModuleDIContainer.BindSingletonFromInstance<ICoreMenuModule, CoreModule>(this);
        }

        private void InstallGlobalBindings(GlobalInstallerBase globalInstaller)
        {
            globalInstaller.InstallBindings(m_CoreModuleDIContainer);
        }

        public static IEntryPoint Create(GlobalInstallerBase globalInstaller = null)
        {
            CoreModule core = new CoreModule();

            if (globalInstaller != null)
            {
                core.InstallGlobalBindings(globalInstaller);
            }

            return core;
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
            m_CoreModuleDIContainer.BindSingleton<IFieldModule, TConcrete>();
        }

        T ICoreMenuModule.GetInstance<T>()
        {
            return m_CoreModuleDIContainer.Resolve<T>();
        }

        object ICoreMenuModule.GetInstance(Type type)
        {
            return m_CoreModuleDIContainer.Resolve(type);
        }

        void ICoreMenuModule.ResetModule<TConcrete>()
        {
            m_CoreModuleDIContainer.BindSingleton<IMenuModule, TConcrete>();
        }

        private async Task LoadModuleAsync<T>(IModuleArgs args) where T : IModule
        {
            await m_CurrentModule.OnExitAsync();

            string sceneName = m_ModuleNames[typeof(T)];

            await SceneManager.LoadSceneAsync(sceneName);

            DIContainer sceneContainer = new DIContainer();

            SceneInstallerMonoBehaviour sceneInstallerMonoBehaviour = Object.FindFirstObjectByType<SceneInstallerMonoBehaviour>();
            SceneInstallerBase          sceneInstaller              = sceneInstallerMonoBehaviour.SceneInstaller;
            sceneInstaller.InstallBindings(sceneContainer);

            m_CoreModuleDIContainer.SetFallback(sceneContainer);

            m_CurrentModule = m_CoreModuleDIContainer.Resolve<T>();

            await m_CurrentModule.OnEnterAsync(args);
        }

        void IDIContainer.SetFallback(IDIContainer fallback)
        {
            m_GlobalContainer.SetFallback(fallback);
        }

        IDIContainer IDIContainer.GetFallback()
        {
            return m_GlobalContainer.GetFallback();
        }

        bool IDIContainer.TryGetBinding(Type type, out Func<object> creator)
        {
            return m_GlobalContainer.TryGetBinding(type, out creator);
        }

        void IDIContainer.BindTransient<TInterface, TConcrete>()
        {
            m_GlobalContainer.BindTransient<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingleton<TInterface, TConcrete>()
        {
            if (typeof(IModule).IsAssignableFrom(typeof(TInterface)))
            {
                m_ModuleNames[typeof(TInterface)] = typeof(TConcrete).Name;
            }

            m_GlobalContainer.BindSingleton<TInterface, TConcrete>();
        }

        void IDIContainer.BindSingletonFromInstance<TInterface, TConcrete>(TConcrete instance)
        {
            m_GlobalContainer.BindSingletonFromInstance<TInterface, TConcrete>(instance);
        }

        T IDIContainer.Resolve<T>()
        {
            return m_GlobalContainer.Resolve<T>();
        }

        object IDIContainer.Resolve(Type type)
        {
            return m_GlobalContainer.Resolve(type);
        }
    }
}