using System;
using System.IO;
using System.Threading.Tasks;
using RPGFramework.Core.Data;
using RPGFramework.Core.Databases;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.UI;
using RPGFramework.Core.Input;
using RPGFramework.Core.Rendering;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;
using RPGFramework.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RPGFramework.Core
{
    public static class CoreModuleBuilder
    {
        public static IEntryPoint Create(GlobalInstallerBase globalInstaller)
        {
            return CoreModule.Create(globalInstaller);
        }
    }

    internal class CoreModule : IEntryPoint, ICoreModule
    {
        private readonly IDIContainer m_GlobalContainer;

        private ISceneDatabase m_SceneDatabase;

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

        public static IEntryPoint Create(GlobalInstallerBase globalInstaller)
        {
            CoreModule core = new CoreModule();

            InstallCoreBindings(core, core.m_GlobalContainer);

            globalInstaller.InstallBindings(core.m_GlobalContainer);

            globalInstaller.Bootstrap(core.m_SceneResolver);

            core.m_SceneDatabase = core.m_SceneResolver.Resolve<ISceneDatabase>();

            return core;
        }

        Task IEntryPoint.StartGameAsync<T>(IModuleArgs args)
        {
            return LoadModuleAsync<T>(args);
        }

        Task ICoreModule.LoadModuleAsync<T>(IModuleArgs args)
        {
            return LoadModuleAsync<T>(args);
        }

        Task ICoreModule.LoadModuleAsync(Type type, IModuleArgs args)
        {
            return LoadModuleAsync(type, args);
        }

        void ICoreModule.ResetModule<TInterface, TConcrete>()
        {
            m_GlobalContainer.ForceBindSingleton<TInterface, TConcrete>();
        }

        Task ICoreModule.ResumeModuleAsync()
        {
            ISaveDataService saveDataService = m_SceneResolver.Resolve<ISaveDataService>();
            IModuleResumeMap moduleResumeMap = m_SceneResolver.Resolve<IModuleResumeMap>();

            if (!saveDataService.TryGetSection(FrameworkSaveSectionDatabase.RESUME_DATA, out SaveSection<RuntimeResumeData> runtimeResumeDataSection))
            {
                throw new InvalidDataException($"{nameof(ICoreModule)}::{nameof(ICoreModule.ResumeModuleAsync)} Config data not found in save data");
            }

            RuntimeResumeData runtimeResumeData = runtimeResumeDataSection.Data;

            Type        moduleType = moduleResumeMap.GetModuleType(runtimeResumeData.ModuleId);
            IModuleArgs args       = moduleResumeMap.CreateArgs(runtimeResumeData);

            return LoadModuleAsync(moduleType, args);
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

            string sceneName = m_SceneDatabase.GetSceneNameForModule(type);

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

            m_CurrentModule = (IModule)m_SceneResolver.Resolve(type);

            await m_CurrentModule.OnEnterAsync(args);
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
        }
    }
}