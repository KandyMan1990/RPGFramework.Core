using System;
using System.Threading.Tasks;
using RPGFramework.Core.Databases;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.UI;
using RPGFramework.Core.Input;
using RPGFramework.Core.PlayerLoop;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;
using RPGFramework.Core.Transitions;
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

    internal sealed class CoreModule : ICoreModule, IModuleHost
    {
        private readonly IDIContainer         m_GlobalContainer;
        private readonly ModuleStack          m_Modules;
        private readonly byte                 m_InitialModuleId;
        private readonly AudioListenerHandOff m_AudioListeners;

        private ISceneDatabase     m_SceneDatabase;
        private IModuleDatabase    m_ModuleDatabase;
        private IModuleRouter      m_Router;
        private ITransitionService m_Transitions;
        private IDIResolver        m_CurrentResolver;
        private Task               m_Changing;

        private CoreModule(byte initialModuleId)
        {
            DIContainer diContainer = new DIContainer();

            m_GlobalContainer = diContainer;
            m_CurrentResolver = diContainer;
            m_Modules         = new ModuleStack(this);
            m_InitialModuleId = initialModuleId;
            m_AudioListeners  = new AudioListenerHandOff();
            m_Changing        = Task.CompletedTask;

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
            core.m_Transitions    = core.m_CurrentResolver.Resolve<ITransitionService>();

            return core;
        }

        Task ICoreModule.StartAsync()
        {
            Task started = m_Modules.ApplyAsync(ModuleChange.Clear(m_InitialModuleId));

            m_Changing = started;

            return started;
        }

        Task ICoreModule.RequestModuleChangeAsync(byte outcome)
        {
            Task change = ChangeAfterAsync(m_Changing, outcome);

            m_Changing = change;

            return change;
        }

        private async Task ChangeAfterAsync(Task previous, byte outcome)
        {
            await Task.WhenAny(previous);

            ModuleChange change = m_Router.Route(m_Modules.TopModuleId, outcome);

            await m_Modules.ApplyAsync(change);
        }

        async Task<StackedModule> IModuleHost.LoadAsync(byte moduleId, bool alone)
        {
            Type   moduleType = m_ModuleDatabase.GetModuleType(moduleId);
            string sceneName  = m_SceneDatabase.GetSceneNameForModule(moduleType);

            if (alone)
            {
                m_AudioListeners.ForgetAll();

                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
            else
            {
                await m_AudioListeners.LoadOverAsync(sceneName);
            }

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
                m_AudioListeners.HandBack(module.Scene);

                await SceneManager.UnloadSceneAsync(module.Scene);
            }

            module.Container.Dispose();
        }

        void IModuleHost.MakeCurrent(StackedModule module)
        {
            SetCurrentResolver((IDIResolver)module.Container);
        }

        Task IModuleHost.CoverAsync(ModuleChange change)
        {
            return m_Transitions.CoverAsync(change.Cover);
        }

        Task IModuleHost.RevealAsync(ModuleChange change)
        {
            return m_Transitions.RevealAsync(change.Reveal);
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

            container.BindInterfacesToSelfSingleton<InputRouter>();
            container.BindSingleton<ITransitionService, TransitionService>();

            container.BindSingletonFromInstance<ISaveStorage>(new FileSaveStorage(Application.persistentDataPath));
            container.BindInterfacesToSelfSingleton<SaveDataService>();
            container.BindSingleton<ISettingsService, SettingsService>();
            container.BindInterfacesToSelfSingleton<MemoryService>();

            container.BindTransient<IDialogueWindow, DialogueWindow>();
            container.BindTransient<IDialogueWindowUI, DialogueWindowUI>();

            container.BindSingleton<ICurrentModuleStore, CurrentModuleStore>();
            container.BindSingleton<ISaveEnabledStore, SaveEnabledStore>();
            container.BindSingleton<ILocationNameStore, LocationNameStore>();
            container.BindSingleton<IBattleMusicStore, BattleMusicStore>();
            container.BindSingleton<IMusicVolumeStore, MusicVolumeStore>();
            container.BindSingleton<IPausedMusicStore, PausedMusicStore>();
            container.BindSingleton<IFrameClock, UnityFrameClock>();
            container.BindSingleton<IPlayTimeStore, PlayTimeCounter>();
        }
    }
}