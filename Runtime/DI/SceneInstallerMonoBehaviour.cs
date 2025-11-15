using UnityEngine;

namespace RPGFramework.Core.DI
{
    public class SceneInstallerMonoBehaviour : MonoBehaviour
    {
        [SerializeField]
        private SceneInstallerBase m_SceneInstaller;
        
        public SceneInstallerBase SceneInstaller => m_SceneInstaller;
    }
}
