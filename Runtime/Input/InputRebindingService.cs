using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.Memory;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Input
{
    internal sealed class InputRebindingService : IInputRebindingService, IInputBindingsLoader
    {
        private const float CANCEL_AFTER_SECONDS = 5f;
        private const float WAIT_FOR_ANOTHER_SECONDS = 0.1f;

        // A direction or a stick is the way the player moves, never a button.
        private static readonly string[] m_NeverBound =
        {
            "<Mouse>",
            "<Gamepad>/dpad/*",
            "<Gamepad>/leftStick/*",
            "<Gamepad>/rightStick/*"
        };

        private readonly IInputActionsProvider m_Actions;
        private readonly InputBindingsStore    m_Store;

        internal InputRebindingService(IInputActionsProvider actions, IMemoryService memory, IVariableMap map)
        {
            m_Actions = actions;
            m_Store   = new InputBindingsStore(memory, map);
        }

        void IInputBindingsLoader.Load()
        {
            InputActionAsset asset = m_Actions.Actions;

            if (asset == null)
            {
                return;
            }

            for (int i = 0; i < m_Store.Capacity; i++)
            {
                if (!m_Store.TryRead(i, out Guid binding, out string path))
                {
                    continue;
                }

                int index = asset.FindBinding(new InputBinding { id = binding }, out InputAction action);

                if (index >= 0)
                {
                    action.ApplyBindingOverride(index, path);
                }
            }
        }

        string IInputRebindingService.ControlName(RebindableControl control, string scheme)
        {
            return TryFindFirst(control, scheme, out InputAction action, out int index) ? action.GetBindingDisplayString(index) : string.Empty;
        }

        async Task<RebindOutcome> IInputRebindingService.RebindAsync(RebindableControl control, string scheme, IReadOnlyList<RebindableControl> others)
        {
            if (!TryFindFirst(control, scheme, out InputAction action, out int index))
            {
                return RebindOutcome.Cancelled;
            }

            string            oldPath = action.bindings[index].effectivePath;
            List<InputAction> enabled = DisableAll(action.actionMap.asset);
            bool              bound;

            // An action is rebound only while off; every other is off too, so the press is not also acted on.
            try
            {
                bound = await WaitForControl(action, index, action.actionMap.asset.FindControlScheme(scheme));
            }
            finally
            {
                for (int i = 0; i < enabled.Count; i++)
                {
                    enabled[i].Enable();
                }
            }

            if (!bound)
            {
                return RebindOutcome.Cancelled;
            }

            string newPath = action.bindings[index].effectivePath;

            Bind(control, scheme, newPath);

            if (newPath == oldPath)
            {
                Store();

                return RebindOutcome.Bound;
            }

            // A control displaced takes the key the one displacing it left, through every map they share, and so on down the
            // chain, each flipping between the two keys.
            List<RebindableControl> moved   = new List<RebindableControl> { control };
            List<string>            movedTo = new List<string> { newPath };

            for (int m = 0; m < moved.Count; m++)
            {
                string left = movedTo[m] == newPath ? oldPath : newPath;

                for (int i = 0; i < others.Count; i++)
                {
                    if (IndexOf(moved, others[i]) < 0 && IsBoundInASharedMap(others[i], moved[m], scheme, movedTo[m]))
                    {
                        moved.Add(others[i]);
                        movedTo.Add(left);
                    }
                }
            }

            for (int m = 1; m < moved.Count; m++)
            {
                Bind(moved[m], scheme, movedTo[m]);
            }

            for (int m = 0; m < moved.Count; m++)
            {
                if (Collides(moved[m], scheme, others))
                {
                    for (int back = 0; back < moved.Count; back++)
                    {
                        Bind(moved[back], scheme, movedTo[back] == newPath ? oldPath : newPath);
                    }

                    return RebindOutcome.Refused;
                }
            }

            Store();

            return RebindOutcome.Bound;
        }

        void IInputRebindingService.ResetAll()
        {
            m_Actions.Actions?.RemoveAllBindingOverrides();

            Store();
        }

        private static Task<bool> WaitForControl(InputAction action, int index, InputControlScheme? scheme)
        {
            TaskCompletionSource<bool> done = new TaskCompletionSource<bool>();

            // No key cancels, so every key can be bound, Escape included: waiting does, and the key already bound keeps it.
            InputActionRebindingExtensions.RebindingOperation operation = action.PerformInteractiveRebinding(index)
                                                                                .WithTimeout(CANCEL_AFTER_SECONDS)
                                                                                .OnMatchWaitForAnother(WAIT_FOR_ANOTHER_SECONDS)
                                                                                .OnComplete(finished => Finish(finished, done, true))
                                                                                .OnCancel(finished => Finish(finished, done, false));

            for (int i = 0; i < m_NeverBound.Length; i++)
            {
                operation.WithControlsExcluding(m_NeverBound[i]);
            }

            for (int i = 0; scheme.HasValue && i < scheme.Value.deviceRequirements.Count; i++)
            {
                operation.WithControlsHavingToMatchPath(scheme.Value.deviceRequirements[i].controlPath);
            }

            operation.Start();

            return done.Task;
        }

        // Disposed in its own callback, as Unity's rebinding sample does.
        private static void Finish(InputActionRebindingExtensions.RebindingOperation operation, TaskCompletionSource<bool> done, bool bound)
        {
            operation.Dispose();
            done.TrySetResult(bound);
        }

        // A binding put back to the control the game's actions give it carries no override, so none is stored.
        private void Bind(RebindableControl control, string scheme, string path)
        {
            for (int i = 0; i < control.Actions.Length; i++)
            {
                if (!TryFind(control.Actions[i], control.Part, scheme, out InputAction action, out int index))
                {
                    continue;
                }

                if (action.bindings[index].path == path)
                {
                    action.RemoveBindingOverride(index);
                }
                else
                {
                    action.ApplyBindingOverride(index, path);
                }
            }
        }

        private void Store()
        {
            InputActionAsset asset  = m_Actions.Actions;
            int              record = 0;

            for (int map = 0; asset != null && map < asset.actionMaps.Count; map++)
            {
                for (int i = 0; i < asset.actionMaps[map].bindings.Count && record < m_Store.Capacity; i++)
                {
                    InputBinding binding = asset.actionMaps[map].bindings[i];

                    if (!string.IsNullOrEmpty(binding.overridePath) && m_Store.Write(record, binding.id, binding.overridePath))
                    {
                        record++;
                    }
                }
            }

            for (; record < m_Store.Capacity; record++)
            {
                m_Store.Clear(record);
            }
        }

        private bool TryFindFirst(RebindableControl control, string scheme, out InputAction action, out int index)
        {
            action = null;
            index  = -1;

            for (int i = 0; i < control.Actions.Length; i++)
            {
                if (TryFind(control.Actions[i], control.Part, scheme, out action, out index))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryFind(string actionPath, string part, string scheme, out InputAction action, out int index)
        {
            action = m_Actions.Actions?.FindAction(actionPath);
            index  = -1;

            for (int i = 0; action != null && i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];

                bool isPart = string.IsNullOrEmpty(part)
                    ? !binding.isComposite && !binding.isPartOfComposite
                    : binding.isPartOfComposite && string.Equals(binding.name, part, StringComparison.OrdinalIgnoreCase);

                if (isPart && IsInGroup(binding.groups, scheme))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        private static bool IsInGroup(string groups, string scheme)
        {
            string[] names = (groups ?? string.Empty).Split(InputBinding.Separator);

            return Array.IndexOf(names, scheme) >= 0;
        }

        // Whether another control is on the same control as this one, in any map they both have actions in.
        private bool Collides(RebindableControl control, string scheme, IReadOnlyList<RebindableControl> others)
        {
            for (int i = 0; i < control.Actions.Length; i++)
            {
                if (!TryFind(control.Actions[i], control.Part, scheme, out InputAction action, out int index))
                {
                    continue;
                }

                for (int j = 0; j < others.Count; j++)
                {
                    if (!IsSame(others[j], control) && IsBoundInMap(others[j], action.actionMap.name, scheme, action.bindings[index].effectivePath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int IndexOf(List<RebindableControl> controls, RebindableControl control)
        {
            for (int i = 0; i < controls.Count; i++)
            {
                if (IsSame(controls[i], control))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool IsBoundInASharedMap(RebindableControl other, RebindableControl control, string scheme, string path)
        {
            for (int i = 0; i < control.Actions.Length; i++)
            {
                if (IsBoundInMap(other, MapOf(control.Actions[i]), scheme, path))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsBoundInMap(RebindableControl control, string map, string scheme, string path)
        {
            for (int i = 0; i < control.Actions.Length; i++)
            {
                if (MapOf(control.Actions[i]) == map && TryFind(control.Actions[i], control.Part, scheme, out InputAction action, out int index) &&
                    action.bindings[index].effectivePath == path)
                {
                    return true;
                }
            }

            return false;
        }

        private static string MapOf(string actionPath)
        {
            int slash = actionPath.IndexOf('/');

            return slash >= 0 ? actionPath.Substring(0, slash) : string.Empty;
        }

        private static bool IsSame(RebindableControl a, RebindableControl b)
        {
            return a.Actions == b.Actions && a.Part == b.Part;
        }

        private static List<InputAction> DisableAll(InputActionAsset asset)
        {
            List<InputAction> enabled = new List<InputAction>();

            for (int map = 0; map < asset.actionMaps.Count; map++)
            {
                for (int i = 0; i < asset.actionMaps[map].actions.Count; i++)
                {
                    InputAction action = asset.actionMaps[map].actions[i];

                    if (action.enabled)
                    {
                        enabled.Add(action);
                        action.Disable();
                    }
                }
            }

            return enabled;
        }
    }
}
