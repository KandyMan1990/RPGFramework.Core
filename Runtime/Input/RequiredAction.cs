#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Input
{
    /// <summary>How a module reads an action: pressed, or as a direction held, a two-dimensional value.</summary>
    public enum RequiredActionKind
    {
        Button,
        Direction
    }

    /// <summary>
    /// An action a module reads by name, which the map its scene's <see cref="InputAdapter" /> routes must therefore
    /// hold, of the kind it is read as, and bound to something.
    /// </summary>
    public readonly struct RequiredAction
    {
        internal readonly string             Name;
        internal readonly RequiredActionKind Kind;
        internal readonly string             Description;

        public RequiredAction(string name, RequiredActionKind kind, string description)
        {
            Name        = name;
            Kind        = kind;
            Description = description;
        }
    }

    /// <summary>
    /// A module's list of the actions it reads, declared beside the code that reads them so each name is written once.
    /// The editor finds every implementation with <c>TypeCache</c>, as it finds <see cref="IRequiredVariables" />, so it
    /// needs a public parameterless constructor; a scene holding <see cref="Module" /> is checked against it in the
    /// adapter's inspector, as the scene loads in play mode, and in a player build.
    /// </summary>
    public interface IRequiredActions
    {
        /// <summary>The module's component in its scene — its <c>IModuleBindings</c> — which marks a scene as the module's.</summary>
        Type Module { get; }

        /// <summary>The map the module's adapter must route, or null for any: the menus' is UI Toolkit's <c>UI</c>.</summary>
        string Map { get; }

        /// <summary>Whether the module moves with its adapter's movement action, which must then be a direction.</summary>
        bool Moves { get; }

        IReadOnlyList<RequiredAction> Actions { get; }
    }

    /// <summary>Authoring only. Every <see cref="IRequiredActions" /> in the project.</summary>
    internal static class RequiredActions
    {
        internal static IRequiredActions[] FindAll()
        {
            return RequiredVariables.CreateAll<IRequiredActions>();
        }
    }
}
#endif
