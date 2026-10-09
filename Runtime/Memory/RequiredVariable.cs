#if UNITY_EDITOR
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
        internal readonly string        Name;
        internal readonly MemoryBank    Bank;
        internal readonly VariableWidth Width;
        internal readonly string        Description;
        internal readonly ulong         DefaultValue;

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

    /// <summary>
    /// Authoring only. Every <see cref="IRequiredVariables" /> and <see cref="IStartModule" /> in the project,
    /// found by type so a package declares its own without anything having to list it.
    /// </summary>
    internal static class RequiredVariables
    {
        internal static RequiredVariable[] FindAll()
        {
            IRequiredVariables[] sources = CreateAll<IRequiredVariables>();
            int                  count   = 0;

            for (int i = 0; i < sources.Length; i++)
            {
                count += sources[i].Variables.Count;
            }

            RequiredVariable[] required = new RequiredVariable[count];
            int                next     = 0;

            for (int i = 0; i < sources.Length; i++)
            {
                IReadOnlyList<RequiredVariable> variables = sources[i].Variables;

                for (int j = 0; j < variables.Count; j++)
                {
                    required[next++] = variables[j];
                }
            }

            return required;
        }

        internal static IStartModule[] FindStartModules()
        {
            IStartModule[] startModules = CreateAll<IStartModule>();

            return startModules;
        }

        private static T[] CreateAll<T>()
        {
            UnityEditor.TypeCache.TypeCollection types = UnityEditor.TypeCache.GetTypesDerivedFrom<T>();

            int count = 0;

            for (int i = 0; i < types.Count; i++)
            {
                if (IsCreatable(types[i]))
                {
                    count++;
                }
            }

            T[] instances = new T[count];
            int next      = 0;

            for (int i = 0; i < types.Count; i++)
            {
                System.Type type = types[i];

                if (IsCreatable(type))
                {
                    instances[next++] = (T)System.Activator.CreateInstance(type);
                }
            }

            return instances;
        }

        private static bool IsCreatable(System.Type type)
        {
            bool isCreatable = !type.IsAbstract && !type.IsInterface;

            return isCreatable;
        }
    }
}
#endif