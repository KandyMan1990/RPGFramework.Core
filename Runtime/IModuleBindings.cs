using RPGFramework.DI;

namespace RPGFramework.Core
{
    /// <summary>
    /// A module's own bindings, on a component in its scene: Core calls it on every change into the module, after the
    /// game's scene installer, with the scene's container. A module binds its own types with <c>…IfNotRegistered</c>,
    /// so a game binding one of their interfaces first replaces the module's own.
    /// </summary>
    public interface IModuleBindings
    {
        /// <summary>
        /// Binds the module's own types into its scene's container, each only where nothing is bound yet.
        /// </summary>
        void Bind(IDIContainer sceneContainer);
    }
}