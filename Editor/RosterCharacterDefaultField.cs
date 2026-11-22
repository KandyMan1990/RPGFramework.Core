using System;
using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Hashing;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// Who a roster record is at the start of a new game, chosen by name from the project's character databases; the
    /// field holds the name's hash, and None, zero, a record no one wears.
    /// </summary>
    internal sealed class RosterCharacterDefaultField : IVariableDefaultField
    {
        string IVariableDefaultField.VariableName => CoreVariables.ROSTER + "." + CoreVariables.ROSTER_CHARACTER;

        VisualElement IVariableDefaultField.Create(ulong value, Action<ulong> write, IVariableValueContext context)
        {
            IReadOnlyList<string> characters = ((ICharacterNames)new CharacterNames()).Names;
            List<string>          names      = new List<string>(characters.Count + 1) { ProviderNames.NONE };

            for (int i = 0; i < characters.Count; i++)
            {
                names.Add(characters[i]);
            }

            int chosen = value == 0 ? 0 : ProviderNames.IndexOfHash(names, value);

            DropdownField field = new DropdownField(CoreVariables.ROSTER_CHARACTER, names, Mathf.Max(0, chosen));

            if (chosen < 0)
            {
                field.SetValueWithoutNotify(string.Empty);
            }

            field.RegisterValueChangedCallback(e => write(names.IndexOf(e.newValue) == 0 ? 0UL : Fnv1a64.Hash(e.newValue)));

            return field;
        }
    }
}
