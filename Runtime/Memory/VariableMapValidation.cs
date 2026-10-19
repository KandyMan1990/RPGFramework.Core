#if UNITY_EDITOR
using System.Collections.Generic;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// Authoring only. The rules a variable map keeps — names, ids, ranges, records, element defaults and the variables
    /// the framework requires — checked over its variables, so the map's inspector, a field export and a player build
    /// refuse one that breaks them.
    /// </summary>
    internal static class VariableMapValidation
    {
        /// <summary>
        /// A human-readable problem for every duplicate name, overlapping range, negative offset, missing name, count
        /// below one or element default outside its array, for a name scripts cannot use, for a record field that is
        /// misnamed, repeated or empty, for an id that is missing, shared or above <paramref name="lastVariableId" />, for
        /// a required variable the map lacks or declares differently, and for a start module default none of
        /// <paramref name="startModules" /> answers to. An empty list means the map is well formed.
        /// </summary>
        internal static string[] Validate(IReadOnlyList<VariableDefinition> variables, uint lastVariableId, RequiredVariable[] requiredVariables, IStartModule[] startModules)
        {
            List<string>    problems = new List<string>();
            HashSet<string> seen     = new HashSet<string>();

            ValidateIds(variables, lastVariableId, problems);

            for (int i = 0; i < variables.Count; i++)
            {
                VariableDefinition variable = variables[i];

                if (string.IsNullOrWhiteSpace(variable.Name))
                {
                    problems.Add($"[{i}] has no name");
                    continue;
                }

                if (!seen.Add(variable.Name))
                {
                    problems.Add($"'{variable.Name}' is declared more than once");
                }

                if (!IsScriptName(variable.Name))
                {
                    problems.Add($"'{variable.Name}' is not a name scripts can use: letters, digits and underscores, not starting with a digit");
                }

                if (variable.Offset < 0)
                {
                    problems.Add($"'{variable.Name}' has a negative offset ({variable.Offset})");
                }

                if (variable.Count < 1)
                {
                    problems.Add($"'{variable.Name}' has a count of {variable.Count}, and a variable holds at least one value");
                }

                ValidateRecordFields(variable, problems);
                ValidateElementDefaults(variable, problems);

                for (int j = i + 1; j < variables.Count; j++)
                {
                    VariableDefinition other = variables[j];

                    if (variable.Overlaps(other))
                    {
                        problems.Add($"'{variable.Name}' ({variable.Bank} {variable.Offset}..{variable.EndOffset}) overlaps '{other.Name}' ({other.Offset}..{other.EndOffset})");
                    }
                }
            }

            ValidateRequiredVariables(variables, requiredVariables, startModules, problems);

            string[] found = problems.ToArray();

            return found;
        }

        private static void ValidateIds(IReadOnlyList<VariableDefinition> variables, uint lastVariableId, List<string> problems)
        {
            HashSet<uint> ids = new HashSet<uint>();

            for (int i = 0; i < variables.Count; i++)
            {
                VariableDefinition variable = variables[i];

                if (variable.Id == 0 || variable.Id > lastVariableId || !ids.Add(variable.Id))
                {
                    problems.Add($"'{variable.Name}' has no id of its own. Open the map's inspector, which assigns one");
                }

                HashSet<ushort> fieldIds = new HashSet<ushort>();

                for (int j = 0; j < variable.Fields.Count; j++)
                {
                    VariableRecordField field = variable.Fields[j];

                    if (field.Id == 0 || !fieldIds.Add(field.Id))
                    {
                        problems.Add($"'{variable.Name}.{field.Name}' has no id of its own. Open the map's inspector, which assigns one");
                    }
                }
            }
        }

        private static void ValidateRecordFields(VariableDefinition variable, List<string> problems)
        {
            HashSet<string> names = new HashSet<string>();

            for (int i = 0; i < variable.Fields.Count; i++)
            {
                VariableRecordField field = variable.Fields[i];

                if (!IsScriptName(field.Name))
                {
                    problems.Add($"'{variable.Name}' has a field called '{field.Name}', which scripts cannot use: letters, digits and underscores, not starting with a digit");
                }

                if (!names.Add(field.Name))
                {
                    problems.Add($"'{variable.Name}' has more than one field called '{field.Name}'");
                }

                if (field.Count < 1)
                {
                    problems.Add($"'{variable.Name}.{field.Name}' has a count of {field.Count}, and a field holds at least one value");
                }
            }
        }

        private static void ValidateElementDefaults(VariableDefinition variable, List<string> problems)
        {
            HashSet<string> described = new HashSet<string>();

            for (int i = 0; i < variable.ElementDefaults.Count; i++)
            {
                VariableElementDefault elementDefault = variable.ElementDefaults[i];

                string where = variable.IsRecord
                                   ? $"[{elementDefault.Index}].{elementDefault.Field}[{elementDefault.FieldIndex}]"
                                   : $"[{elementDefault.Index}]";

                if (elementDefault.Index < 0 || elementDefault.Index >= variable.Count)
                {
                    problems.Add($"'{variable.Name}' has a default for {where}, outside its {variable.Count} element(s)");
                }

                if (!variable.IsRecord && elementDefault.Field.Length > 0)
                {
                    problems.Add($"'{variable.Name}' is not a record, and has a default for a field called '{elementDefault.Field}'");
                }

                if (variable.IsRecord)
                {
                    if (!variable.TryGetField(elementDefault.Field, out VariableRecordField field, out int _))
                    {
                        problems.Add($"'{variable.Name}' has a default for {where}, and has no field called '{elementDefault.Field}'");
                    }
                    else if (elementDefault.FieldIndex < 0 || elementDefault.FieldIndex >= field.Count)
                    {
                        problems.Add($"'{variable.Name}' has a default for {where}, outside the field's {field.Count} element(s)");
                    }
                }

                if (!described.Add(where))
                {
                    problems.Add($"'{variable.Name}' has more than one default for {where}");
                }
            }
        }

        private static void ValidateRequiredVariables(IReadOnlyList<VariableDefinition> variables, RequiredVariable[] requiredVariables, IStartModule[] startModules, List<string> problems)
        {
            for (int i = 0; i < requiredVariables.Length; i++)
            {
                RequiredVariable required = requiredVariables[i];

                if (!TryFind(variables, required.Name, out VariableDefinition variable))
                {
                    problems.Add($"'{required.Name}' is required by the framework and is not declared — use Add Missing Required Variables. {required.Description}");
                    continue;
                }

                if (variable.Bank != required.Bank || variable.Width != required.Width)
                {
                    problems.Add($"'{required.Name}' is declared as {variable.Bank} {variable.Width}, but the framework reads it as {required.Bank} {required.Width}");
                }

                if (variable.Count != 1)
                {
                    problems.Add($"'{required.Name}' is declared as an array of {variable.Count}, but the framework reads it as a single value");
                }

                if (variable.IsRecord)
                {
                    problems.Add($"'{required.Name}' is declared as a record, but the framework reads it as a single {required.Width}");
                }
            }

            if (!TryFind(variables, CoreVariables.CURRENT_MODULE, out VariableDefinition currentModule))
            {
                return;
            }

            byte startModule = (byte)currentModule.DefaultValue;

            for (int i = 0; i < startModules.Length; i++)
            {
                IStartModule module = startModules[i];

                if (module.ModuleId == startModule)
                {
                    return;
                }
            }

            problems.Add($"'{CoreVariables.CURRENT_MODULE}' defaults to module [{startModule}], which is not a module a game can begin in. Choose one in the Variable Map inspector");
        }

        /// <summary>
        /// Letters, digits and underscores, not starting with a digit — what a variable or a record field can be called,
        /// since scripts name them in text beside <c>[</c> and <c>.</c>.
        /// </summary>
        internal static bool IsScriptName(string name)
        {
            bool isScriptName = !string.IsNullOrEmpty(name) && !char.IsDigit(name[0]);

            if (isScriptName)
            {
                for (int i = 0; i < name.Length; i++)
                {
                    char c = name[i];

                    if (!char.IsLetterOrDigit(c) && c != '_')
                    {
                        isScriptName = false;
                        break;
                    }
                }
            }

            return isScriptName;
        }

        // Last wins, as the map's own lookup by name does.
        private static bool TryFind(IReadOnlyList<VariableDefinition> variables, string name, out VariableDefinition found)
        {
            found = null;

            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Name == name)
                {
                    found = variables[i];
                }
            }

            bool any = found != null;

            return any;
        }
    }
}
#endif
