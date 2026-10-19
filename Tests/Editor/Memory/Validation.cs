using System;
using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Tests.Memory
{
    internal static class Validation
    {
        /// <summary>What is wrong with a map's variables by the map's own rules, leaving out the framework's required variables.</summary>
        internal static string[] Problems(IVariableMap map)
        {
            string[] problems = VariableMapValidation.Validate(map.Variables, map.LastVariableId, Array.Empty<RequiredVariable>(), Array.Empty<IStartModule>());

            return problems;
        }
    }
}
