using System;
using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Hashing;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// The track every battle plays in a new game, until a script sets another, chosen by name from the project's music
    /// providers; the variable holds the name's hash, and None, zero.
    /// </summary>
    internal sealed class BattleMusicDefaultField : IVariableDefaultField
    {
        string IVariableDefaultField.VariableName => CoreVariables.BATTLE_MUSIC;

        VisualElement IVariableDefaultField.Create(ulong value, Action<ulong> write, IVariableValueContext context)
        {
            List<string> names = ProviderNames.Music();

            names.Insert(0, ProviderNames.NONE);

            int chosen = value == 0 ? 0 : ProviderNames.IndexOfHash(names, value);

            DropdownField field = new DropdownField("Battle music", names, Mathf.Max(0, chosen));

            if (chosen < 0)
            {
                field.SetValueWithoutNotify(string.Empty);
            }

            field.RegisterValueChangedCallback(e => write(names.IndexOf(e.newValue) == 0 ? 0UL : Fnv1a64.Hash(e.newValue)));

            return field;
        }
    }
}