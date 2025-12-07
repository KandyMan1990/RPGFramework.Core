using System;
using System.IO;
using System.Threading.Tasks;
using RPGFramework.Core.SharedTypes;
using RPGFramework.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RPGFramework.Core
{
    public class CoreModule : IEntryPoint, ICoreModule
    {
        private readonly IDIContainer        m_CoreModuleDIContainer;
        private readonly IModuleNameProvider m_ModuleNameProvider;
        private readonly ICoreModule         m_CoreModule;

        private IDIContainer m_SceneContainer;
        private IModule      m_CurrentModule;

        private CoreModule()
        {
            CoreModuleDIContainer coreModuleDIContainer = new CoreModuleDIContainer();

            m_CoreModuleDIContainer = coreModuleDIContainer;
            m_ModuleNameProvider    = coreModuleDIContainer;
            m_CurrentModule         = new NullModule();

            m_CoreModuleDIContainer.BindSingletonFromInstance<ICoreModule>(this);

            m_CoreModule = this;
        }

        public static IEntryPoint Create(GlobalInstallerBase globalInstaller)
        {
            CoreModule core = new CoreModule();

            globalInstaller.InstallBindings(core.m_CoreModuleDIContainer);

            return core;
        }

        Task IEntryPoint.StartGameAsync<T>(IModuleArgs args)
        {
            return LoadModuleAsync<T>(args);
        }

        T ICoreModule.GetInstance<T>()
        {
            return m_SceneContainer.Resolve<T>();
        }

        object ICoreModule.GetInstance(Type type)
        {
            return m_SceneContainer.Resolve(type);
        }

        Task ICoreModule.LoadModuleAsync<T>(IModuleArgs args)
        {
            return m_CoreModule.LoadModuleAsync<T>(args);
        }

        Task ICoreModule.LoadModuleAsync(Type type, IModuleArgs args)
        {
            return LoadModuleAsync(type, args);
        }

        void ICoreModule.ResetModule<TInterface, TConcrete>()
        {
            m_CoreModuleDIContainer.BindSingleton<TInterface, TConcrete>();
        }

        private Task LoadModuleAsync<T>(IModuleArgs args) where T : IModule
        {
            return LoadModuleAsync(typeof(T), args);
        }

        private async Task LoadModuleAsync(Type type, IModuleArgs args)
        {
            if (type.GetInterface(nameof(IModule)) == null)
            {
                throw new InvalidDataException($"{nameof(ICoreModule)}::{nameof(LoadModuleAsync)} [{type}] must be assignable from {nameof(IModule)}");
            }
            
            await m_CurrentModule.OnExitAsync();

            string sceneName = m_ModuleNameProvider.GetModuleName(type);

            await SceneManager.LoadSceneAsync(sceneName);

            m_SceneContainer = new DIContainer();

            SceneInstallerMonoBehaviour sceneInstallerMonoBehaviour = Object.FindFirstObjectByType<SceneInstallerMonoBehaviour>();
            SceneInstallerBase          sceneInstaller              = sceneInstallerMonoBehaviour.SceneInstaller;
            sceneInstaller.InstallBindings(m_SceneContainer);

            m_SceneContainer.SetFallback(m_CoreModuleDIContainer);

            m_CurrentModule = (IModule)m_SceneContainer.Resolve(type);

            await m_CurrentModule.OnEnterAsync(args);
        }
    }
}