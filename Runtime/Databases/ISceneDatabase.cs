using System;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Databases
{
    /// <summary>
    /// Used to get the name of a Unity Scene a module uses
    /// </summary>
    public interface ISceneDatabase
    {
        /// <summary>
        /// Get the Unity Scene name to load when transitioning to IModule T
        /// </summary>
        /// <typeparam name="T">The IModule we're about to transition to</typeparam>
        /// <returns>The name of the Unity Scene associated with IModule T</returns>
        string GetSceneNameForModule<T>() where T : IModule;

        /// <summary>
        /// Get the Unity Scene name to load when transitioning to an IModule
        /// </summary>
        /// <param name="type">The IModule we're about to transition to</param>
        /// <returns>The name of the Unity Scene associated with the IModule type</returns>
        string GetSceneNameForModule(Type type);
    }
}