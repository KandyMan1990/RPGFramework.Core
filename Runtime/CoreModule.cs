using System;
using System.Threading.Tasks;
using RPGFramework.Core.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RPGFramework.Core
{
    public class CoreModule : IEntryPoint, ICoreFieldModule, ICoreMenuModule
    {
        private readonly DIContainer m_GlobalContainer;

        private IModule m_CurrentModule;

        private CoreModule()
        {
            m_GlobalContainer = new DIContainer();
            m_CurrentModule   = new NullModule();

            m_GlobalContainer.BindSingletonFromInstance<ICoreFieldModule, CoreModule>(this);
            m_GlobalContainer.BindSingletonFromInstance<ICoreMenuModule, CoreModule>(this);
        }

        private void InstallGlobalBindings(GlobalInstallerBase globalInstaller)
        {
            globalInstaller.InstallBindings(m_GlobalContainer);
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
            m_GlobalContainer.BindModule<IFieldModule, TConcrete>();
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
            m_GlobalContainer.BindModule<IMenuModule, TConcrete>();
        }

        private async Task LoadModuleAsync<T>(IModuleArgs args) where T : IModule
        {
            await m_CurrentModule.OnExitAsync();

            string sceneName = m_GlobalContainer.GetModuleName<T>();

            await SceneManager.LoadSceneAsync(sceneName);

            DIContainer sceneContainer = new DIContainer();

            SceneInstallerMonoBehaviour sceneInstallerMonoBehaviour = Object.FindFirstObjectByType<SceneInstallerMonoBehaviour>();
            SceneInstallerBase          sceneInstaller              = sceneInstallerMonoBehaviour.SceneInstaller;
            sceneInstaller.InstallBindings(sceneContainer);

            m_GlobalContainer.SetFallback(sceneContainer);

            m_CurrentModule = m_GlobalContainer.Resolve<T>();

            await m_CurrentModule.OnEnterAsync(args);
        }
    }
}