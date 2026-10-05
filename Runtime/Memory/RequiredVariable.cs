using System.Collections.Generic;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// A variable something in the framework reads and writes by name, which every game's map must therefore
    /// declare. A new <see cref="VariableMapAsset" /> is created holding them, and exporting refuses a map that
    /// lacks one or declares it at the wrong bank or width.
    /// </summary>
    public readonly struct RequiredVariable
    {
        public readonly string        Name;
        public readonly MemoryBank    Bank;
        public readonly VariableWidth Width;
        public readonly string        Description;
        public readonly ulong         DefaultValue;

        public RequiredVariable(string        name,
                                MemoryBank    bank,
                                VariableWidth width,
                                string        description,
                                ulong         defaultValue = 0)
        {
            Name         = name;
            Bank         = bank;
            Width        = width;
            Description  = description;
            DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// A package's list of the variables it requires. Declared beside the code that uses them, so the names
    /// are written once; the editor finds every implementation with <c>TypeCache</c>, so it needs a public
    /// parameterless constructor.
    /// </summary>
    public interface IRequiredVariables
    {
        IReadOnlyList<RequiredVariable> Variables { get; }
    }

    /// <summary>
    /// A module a new game can begin in, offered as a choice for <see cref="CoreVariables.CURRENT_MODULE" />'s
    /// default. Found the same way as <see cref="IRequiredVariables" />.
    /// </summary>
    public interface IStartModule
    {
        byte   ModuleId   { get; }
        string ModuleName { get; }
    }
}

#if UNITY_EDITOR
namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// Authoring only. Every <see cref="IRequiredVariables" /> and <see cref="IStartModule" /> in the project,
    /// found by type so a package declares its own without anything having to list it.
    /// </summary>
    public static class RequiredVariables
    {
        public static List<RequiredVariable> FindAll()
        {
            List<RequiredVariable> required = new List<RequiredVariable>();

            foreach (IRequiredVariables source in CreateAll<IRequiredVariables>())
            {
                required.AddRange(source.Variables);
            }

            return required;
        }

        public static List<IStartModule> FindStartModules()
        {
            List<IStartModule> startModules = CreateAll<IStartModule>();

            return startModules;
        }

        private static List<T> CreateAll<T>()
        {
            List<T> instances = new List<T>();

            foreach (System.Type type in UnityEditor.TypeCache.GetTypesDerivedFrom<T>())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                instances.Add((T)System.Activator.CreateInstance(type));
            }

            return instances;
        }
    }
}
#endif