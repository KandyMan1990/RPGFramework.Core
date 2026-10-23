using System;

namespace RPGFramework.Core.Databases
{
    /// <summary>The Unity scene each module runs in, by the module's interface.</summary>
    public interface ISceneDatabase
    {
        /// <summary>The name of the scene to load for the module bound to <paramref name="type" />.</summary>
        string GetSceneNameForModule(Type type);
    }
}
