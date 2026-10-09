using System.Collections.Generic;
using RPGFramework.Core.Memory;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// The module a new game begins in, chosen from the modules that declare themselves somewhere a game can
    /// begin.
    /// </summary>
    internal sealed class CurrentModuleDefaultField : IVariableDefaultField
    {
        string IVariableDefaultField.VariableName => CoreVariables.CURRENT_MODULE;

        VisualElement IVariableDefaultField.Create(SerializedProperty variable)
        {
            SerializedProperty defaultValue = variable.FindPropertyRelative(VariableDefinitionDrawer.DEFAULT_VALUE);
            IStartModule[]     modules      = RequiredVariables.FindStartModules();

            if (modules.Length == 0)
            {
                HelpBox none = new HelpBox("No installed module is somewhere a game can begin.", HelpBoxMessageType.Warning);

                return none;
            }

            List<string> names = new List<string>(modules.Length);
            int          chosen = -1;

            for (int i = 0; i < modules.Length; i++)
            {
                names.Add(modules[i].ModuleName);

                if (modules[i].ModuleId == (byte)defaultValue.ulongValue)
                {
                    chosen = i;
                }
            }

            DropdownField field = new DropdownField("Starts in", names, Mathf.Max(0, chosen));

            if (chosen < 0)
            {
                field.SetValueWithoutNotify($"Module {defaultValue.ulongValue}");
            }

            field.RegisterValueChangedCallback(e => VariableDefinitionDrawer.Write(defaultValue, modules[names.IndexOf(e.newValue)].ModuleId));

            return field;
        }
    }
}
