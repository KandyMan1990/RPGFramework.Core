using System;
using System.Threading.Tasks;
using RPGFramework.Core.Databases;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.UI;
using RPGFramework.Core.Input;
using RPGFramework.Core.Rendering;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;
using RPGFramework.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RPGFramework.Core
{
    public static class CoreModuleBuilder
    {
        public static ICoreModule Create(GlobalInstallerBase globalInstaller, byte initialModuleId)
        {
            return CoreModule.Create(globalInstaller, initialModuleId);
        }
    }

    internal class CoreModule : ICoreModule
    {
        private readonly IDIContainer m_GlobalContainer;

        private ISceneDatabase     m_SceneDatabase;
        private IChangeModuleStore m_ChangeModuleStore;
        private IModuleDatabase    m_ModuleDatabase;

        private IDIContainer m_SceneContainer;
        private IDIResolver  m_SceneResolver;
        private IModule      m_CurrentModule;

        private CoreModule()
        {
            DIContainer diContainer = new DIContainer();

            m_GlobalContainer = diContainer;
            m_SceneContainer  = new NullDIContainer();
            m_SceneResolver   = diContainer;
            m_CurrentModule   = new NullModule();

            Application.quitting += OnApplicationQuit;
        }

        public static ICoreModule Create(GlobalInstallerBase globalInstaller, byte initialModuleId)
        {
            CoreModule core = new CoreModule();

            InstallCoreBindings(core, core.m_GlobalContainer);

            globalInstaller.InstallBindings(core.m_GlobalContainer);

            globalInstaller.Bootstrap(core.m_SceneResolver);

            core.m_SceneDatabase     = core.m_SceneResolver.Resolve<ISceneDatabase>();
            core.m_ChangeModuleStore = core.m_SceneResolver.Resolve<IChangeModuleStore>();
            core.m_ModuleDatabase    = core.m_SceneResolver.Resolve<IModuleDatabase>();

            core.m_ChangeModuleStore.SetModuleId(initialModuleId);

            return core;
        }

        async Task ICoreModule.RequestModuleChangeAsync()
        {
            await m_CurrentModule.OnExitAsync();

            byte   moduleId   = m_ChangeModuleStore.GetModuleId;
            Type   moduleType = m_ModuleDatabase.GetModuleType(moduleId);
            string sceneName  = m_SceneDatabase.GetSceneNameForModule(moduleType);

            await SceneManager.LoadSceneAsync(sceneName);

            m_SceneContainer.Dispose();

            DIContainer sceneContainer = new DIContainer();

            m_SceneContainer = sceneContainer;
            m_SceneResolver  = sceneContainer;

            SceneInstallerMonoBehaviour sceneInstallerMonoBehaviour = Object.FindAnyObjectByType<SceneInstallerMonoBehaviour>();
            SceneInstallerBase          sceneInstaller              = sceneInstallerMonoBehaviour.SceneInstaller;
            sceneInstaller.InstallBindings(m_SceneContainer);

            m_GlobalContainer.ForceBindSingletonFromInstance<IDIResolver>(m_SceneResolver);

            m_SceneContainer.SetFallback(m_GlobalContainer);

            m_CurrentModule = (IModule)m_SceneResolver.Resolve(moduleType);

            // TODO: allow a module to register its own internal types so we don't have to make them public and registered in scene installers

            await m_CurrentModule.OnEnterAsync();
        }

        void ICoreModule.ResetModule<TInterface, TConcrete>()
        {
            m_GlobalContainer.ForceBindSingleton<TInterface, TConcrete>();
        }

        private void OnApplicationQuit()
        {
            m_SceneContainer.Dispose();
            m_GlobalContainer.Dispose();
        }

        private static void InstallCoreBindings(ICoreModule core, IDIContainer container)
        {
            container.BindSingletonFromInstance<ICoreModule>(core);

            container.BindSingleton<IInputRouter, InputRouter>();
            container.BindSingleton<IScreenFadeService, ScreenFadeService>();

            container.BindSingleton<ISaveDataService, SaveDataService>();
            container.BindSingleton<IMemoryService, MemoryService>();

            container.BindSingleton<IDialogueWindow, DialogueWindow>();
            container.BindSingleton<IDialogueWindowUI, DialogueWindowUI>();

            container.BindSingleton<IChangeModuleStore, ChangeModuleStore>();
            container.BindSingleton<IResumeModuleStore, ResumeModuleStore>();
        }
    }
}