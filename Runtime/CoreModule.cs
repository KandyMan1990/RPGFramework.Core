using System.Threading.Tasks;
using RPGFramework.DI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RPGFramework.Core
{
    public partial class CoreModule : IEntryPoint
    {
        private readonly IDIContainer        m_CoreModuleDIContainer;
        private readonly IModuleNameProvider m_ModuleNameProvider;

        private IModule m_CurrentModule;

        private CoreModule()
        {
            CoreModuleDIContainer coreModuleDIContainer = new CoreModuleDIContainer();

            m_CoreModuleDIContainer = coreModuleDIContainer;
            m_ModuleNameProvider    = coreModuleDIContainer;
            m_CurrentModule         = new NullModule();

            m_CoreModuleDIContainer.BindSingletonFromInstance<ICoreMenuModule, CoreModule>(this);
            m_CoreModuleDIContainer.BindSingletonFromInstance<ICoreFieldModule, CoreModule>(this);
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
            MenuModuleArgs args = new MenuModuleArgs(typeof(IBeginMenu));

            return LoadModuleAsync<IMenuModule>(args);
        }

        private async Task LoadModuleAsync<T>(IModuleArgs args) where T : IModule
        {
            await m_CurrentModule.OnExitAsync();

            string sceneName = m_ModuleNameProvider.GetModuleName<T>();

            await SceneManager.LoadSceneAsync(sceneName);

            DIContainer sceneContainer = new DIContainer();

            SceneInstallerMonoBehaviour sceneInstallerMonoBehaviour = Object.FindFirstObjectByType<SceneInstallerMonoBehaviour>();
            SceneInstallerBase          sceneInstaller              = sceneInstallerMonoBehaviour.SceneInstaller;
            sceneInstaller.InstallBindings(sceneContainer);

            m_CoreModuleDIContainer.SetFallback(sceneContainer);

            m_CurrentModule = m_CoreModuleDIContainer.Resolve<T>();

            await m_CurrentModule.OnEnterAsync(args);
        }
    }
}