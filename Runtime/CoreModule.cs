using System;
using System.Threading.Tasks;
using RPGFramework.Core.Databases;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.UI;
using RPGFramework.Core.Input;
using RPGFramework.Core.Rendering;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;
using RPGFramework.DI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGFramework.Core
{
    public static class CoreModuleFactory
    {
        public static Task<ICoreModule> Create(GlobalInstallerBase globalInstaller, byte initialModuleId)
        {
            return CoreModule.Create(globalInstaller, initialModuleId);
        }
    }

    internal class CoreModule : ICoreModule, IModuleHost
    {
        private readonly IDIContainer m_GlobalContainer;
        private readonly ModuleStack  m_Modules;
        private readonly byte         m_InitialModuleId;

        private ISceneDatabase  m_SceneDatabase;
        private IModuleDatabase m_ModuleDatabase;
        private IModuleRouter   m_Router;
        private IDIResolver     m_CurrentResolver;

        private CoreModule(byte initialModuleId)
        {
            DIContainer diContainer = new DIContainer();

            m_GlobalContainer = diContainer;
            m_CurrentResolver = diContainer;
            m_Modules         = new ModuleStack(this);
            m_InitialModuleId = initialModuleId;

            Application.quitting += OnApplicationQuit;
        }

        public static async Task<ICoreModule> Create(GlobalInstallerBase globalInstaller, byte initialModuleId)
        {
            CoreModule core = new CoreModule(initialModuleId);

            InstallCoreBindings(core, core.m_GlobalContainer);

            globalInstaller.InstallBindings(core.m_GlobalContainer);

            await globalInstaller.Bootstrap(core.m_CurrentResolver);

            core.m_SceneDatabase  = core.m_CurrentResolver.Resolve<ISceneDatabase>();
            core.m_ModuleDatabase = core.m_CurrentResolver.Resolve<IModuleDatabase>();
            core.m_Router         = core.m_CurrentResolver.Resolve<IModuleRouter>();

            return core;
        }

        Task ICoreModule.StartAsync()
        {
            return m_Modules.ApplyAsync(ModuleChange.Clear(m_InitialModuleId));
        }

        Task ICoreModule.RequestModuleChangeAsync(byte outcome)
        {
            ModuleChange change = m_Router.Route(m_Modules.TopModuleId, outcome);

            return m_Modules.ApplyAsync(change);
        }

        async Task<StackedModule> IModuleHost.LoadAsync(byte moduleId, bool alone)
        {
            Type   moduleType = m_ModuleDatabase.GetModuleType(moduleId);
            string sceneName  = m_SceneDatabase.GetSceneNameForModule(moduleType);

            await SceneManager.LoadSceneAsync(sceneName, alone ? LoadSceneMode.Single : LoadSceneMode.Additive);

            Scene        scene          = SceneManager.GetSceneByName(sceneName);
            DIContainer  diContainer    = new DIContainer();
            IDIContainer sceneContainer = diContainer;

            sceneContainer.BindSingletonFromInstance<IModuleScene>(new ModuleScene(scene));
            FindSceneInstaller(scene).InstallBindings(sceneContainer);
            sceneContainer.SetFallback(m_GlobalContainer);

            SetCurrentResolver(diContainer);

            IModule       module  = (IModule)m_CurrentResolver.Resolve(moduleType);
            StackedModule stacked = new StackedModule(moduleId, module, scene, sceneContainer);

            return stacked;
        }

        async Task IModuleHost.UnloadAsync(StackedModule module, bool unloadScene)
        {
            if (unloadScene)
            {
                await SceneManager.UnloadSceneAsync(module.Scene);
            }

            module.Container.Dispose();
        }

        void IModuleHost.MakeCurrent(StackedModule module)
        {
            SetCurrentResolver((IDIResolver)module.Container);
        }

        private static SceneInstallerBase FindSceneInstaller(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                SceneInstallerMonoBehaviour installer = roots[i].GetComponentInChildren<SceneInstallerMonoBehaviour>();

                if (installer != null)
                {
                    SceneInstallerBase sceneInstaller = installer.SceneInstaller;

                    return sceneInstaller;
                }
            }

            throw new InvalidOperationException($"{nameof(CoreModule)}::{nameof(FindSceneInstaller)} Scene [{scene.name}] has no {nameof(SceneInstallerMonoBehaviour)}");
        }

        // Global bindings resolve IDIResolver as the module on top's container, so whatever resolves later finds that
        // module's bindings as well as the global ones.
        private void SetCurrentResolver(IDIResolver resolver)
        {
            m_GlobalContainer.Unbind<IDIResolver>(m_CurrentResolver);
            m_GlobalContainer.BindSingletonFromInstance(resolver);

            m_CurrentResolver = resolver;
        }

        private void OnApplicationQuit()
        {
            foreach (StackedModule module in m_Modules.Modules)
            {
                module.Container.Dispose();
            }

            m_GlobalContainer.Dispose();
        }

        private static void InstallCoreBindings(ICoreModule core, IDIContainer container)
        {
            container.BindSingletonFromInstance<ICoreModule>(core);

            container.BindSingleton<IInputRouter, InputRouter>();
            container.BindSingleton<IScreenFadeService, ScreenFadeService>();

            container.BindSingleton<ISaveDataService, SaveDataService>();
            container.BindSingleton<ISettingsService, SettingsService>();
            container.BindInterfacesToSelfSingleton<MemoryService>();

            container.BindTransient<IDialogueWindow, DialogueWindow>();
            container.BindTransient<IDialogueWindowUI, DialogueWindowUI>();

            container.BindSingleton<ICurrentModuleStore, CurrentModuleStore>();
            container.BindSingleton<ISaveEnabledStore, SaveEnabledStore>();
            container.BindSingleton<ILocationNameStore, LocationNameStore>();
            container.BindSingleton<IPlayTimeStore, PlayTimeCounter>();
        }
    }
}