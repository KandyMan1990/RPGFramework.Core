using System.Collections.Generic;
using RPGFramework.Core.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// Whether a module scene's input adapter routes a map holding every action the scene's module reads by name, as
    /// its <see cref="IRequiredActions" /> declares them. The names are strings in the code and in the game's actions,
    /// so nothing else would notice one missing: the press would simply never arrive.
    /// </summary>
    internal static class InputActionsCheck
    {
        /// <summary>The problems with every adapter in the scene, against the modules the scene holds.</summary>
        internal static void CheckScene(Scene scene, List<string> problems)
        {
            IRequiredActions[] modules  = ModulesIn(scene);
            InputAdapter[]     adapters = AdaptersIn(scene);

            if (adapters.Length == 0 && modules.Length > 0)
            {
                problems.Add($"{scene.name} holds {modules[0].Module.Name}, which reads input, but no {nameof(InputAdapter)}");
            }

            for (int i = 0; i < adapters.Length; i++)
            {
                CheckAdapter(adapters[i], modules, problems);
            }
        }

        /// <summary>The problems with one adapter, against the modules in its scene.</summary>
        internal static void CheckAdapter(InputAdapter adapter, IRequiredActions[] modules, List<string> problems)
        {
            SerializedObject serialized = new SerializedObject(adapter);

            Check((InputActionAsset)serialized.FindProperty("m_Actions").objectReferenceValue,
                  serialized.FindProperty("m_Map").stringValue,
                  serialized.FindProperty("m_Movement").stringValue,
                  InputSystem.actions,
                  modules,
                  $"{adapter.gameObject.scene.name} / '{adapter.name}'",
                  problems);
        }

        internal static void Check(InputActionAsset   actions,
                                   string             mapName,
                                   string             movement,
                                   InputActionAsset   projectWide,
                                   IRequiredActions[] modules,
                                   string             where,
                                   List<string>       problems)
        {
            if (actions == null)
            {
                problems.Add($"{where} is given no input actions. Give it the project-wide actions");
                return;
            }

            if (projectWide == null)
            {
                problems.Add($"The project has no project-wide input actions, so UI Toolkit reads its own defaults rather than '{actions.name}'. Set them in Project Settings > Input System Package");
            }
            else if (actions != projectWide)
            {
                problems.Add($"{where} is given '{actions.name}', not the project-wide actions '{projectWide.name}', which UI Toolkit reads");
            }

            InputActionMap map = actions.FindActionMap(mapName);

            if (map == null)
            {
                problems.Add($"{where} routes the map '{mapName}', which '{actions.name}' does not have");
                return;
            }

            for (int i = 0; i < modules.Length; i++)
            {
                CheckModule(map, movement, modules[i], where, problems);
            }
        }

        /// <summary>The actions a module reads that its adapter's map does not have at all.</summary>
        internal static List<RequiredAction> Missing(InputActionMap map, IRequiredActions[] modules)
        {
            List<RequiredAction> missing = new List<RequiredAction>();

            for (int i = 0; i < modules.Length; i++)
            {
                IReadOnlyList<RequiredAction> actions = modules[i].Actions;

                for (int j = 0; j < actions.Count; j++)
                {
                    if (map.FindAction(actions[j].Name) == null && !missing.Exists(action => action.Name == actions[j].Name))
                    {
                        missing.Add(actions[j]);
                    }
                }
            }

            return missing;
        }

        internal static IRequiredActions[] ModulesIn(Scene scene)
        {
            IRequiredActions[]     declared = RequiredActions.FindAll();
            List<IRequiredActions> modules  = new List<IRequiredActions>();
            GameObject[]           roots    = scene.GetRootGameObjects();

            for (int i = 0; i < declared.Length; i++)
            {
                if (!typeof(Component).IsAssignableFrom(declared[i].Module))
                {
                    continue;
                }

                for (int j = 0; j < roots.Length; j++)
                {
                    if (roots[j].GetComponentInChildren(declared[i].Module, true) != null)
                    {
                        modules.Add(declared[i]);
                        break;
                    }
                }
            }

            return modules.ToArray();
        }

        private static InputAdapter[] AdaptersIn(Scene scene)
        {
            List<InputAdapter> adapters = new List<InputAdapter>();
            GameObject[]       roots    = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                adapters.AddRange(roots[i].GetComponentsInChildren<InputAdapter>(true));
            }

            return adapters.ToArray();
        }

        private static void CheckModule(InputActionMap map, string movement, IRequiredActions module, string where, List<string> problems)
        {
            string reader = module.Module.Name;

            if (module.Map != null && module.Map != map.name)
            {
                problems.Add($"{where} routes the map '{map.name}', but {reader} reads the '{module.Map}' map");
            }

            if (module.Moves)
            {
                InputAction moving = string.IsNullOrEmpty(movement) ? null : map.FindAction(movement);

                if (moving == null)
                {
                    problems.Add($"{where} names no movement action in '{map.name}', and {reader} moves with it");
                }
                else if (!IsDirection(moving))
                {
                    problems.Add($"{where} moves with '{movement}', which is not a direction: make it a Value or Pass Through action of control type Vector2");
                }
            }

            IReadOnlyList<RequiredAction> actions = module.Actions;

            for (int i = 0; i < actions.Count; i++)
            {
                RequiredAction required = actions[i];
                InputAction    action   = map.FindAction(required.Name);

                if (action == null)
                {
                    problems.Add($"'{map.name}' has no '{required.Name}' action, which {reader} reads: {required.Description}. Add it from {where}'s inspector");
                }
                else if (required.Kind == RequiredActionKind.Button && action.type != InputActionType.Button)
                {
                    problems.Add($"'{map.name}' / '{required.Name}' is not a Button action, and {reader} reads it as one");
                }
                else if (required.Kind == RequiredActionKind.Direction && !IsDirection(action))
                {
                    problems.Add($"'{map.name}' / '{required.Name}' is not a direction, and {reader} reads it as one: make it a Value or Pass Through action of control type Vector2");
                }
                else if (!IsBound(action))
                {
                    problems.Add($"'{map.name}' / '{required.Name}' is bound to nothing, so {reader} never hears it: {required.Description}");
                }
            }
        }

        private static bool IsDirection(InputAction action)
        {
            return action.type != InputActionType.Button && action.expectedControlType == "Vector2";
        }

        private static bool IsBound(InputAction action)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];

                if (!binding.isComposite && !string.IsNullOrEmpty(binding.path))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
