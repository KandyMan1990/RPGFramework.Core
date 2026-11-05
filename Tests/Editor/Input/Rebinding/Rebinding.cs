using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Core.Input;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>
    /// A game's actions with a keyboard and a gamepad scheme — a Player map to confirm, open the menu and move in, and a
    /// UI map to submit and delete in — the player's bindings kept in a settings bank, and a player pressing keys on the
    /// Input System's test devices.
    /// </summary>
    internal sealed class Rebinding
    {
        internal const string KEYBOARD = "Keyboard&Mouse";
        internal const string GAMEPAD  = "Gamepad";

        private const int RECORDS = 4;
        private const double WAITED_SECONDS = 6;

        internal readonly RebindableControl Confirm;
        internal readonly RebindableControl Cancel;
        internal readonly RebindableControl Menu;
        internal readonly RebindableControl Delete;
        internal readonly RebindableControl Up;

        private readonly InputTestFixture   m_Input;
        private readonly InputActionAsset   m_Actions;
        private readonly MemoryService      m_Memory;
        private readonly TestVariableMap    m_Map;
        private readonly IInputRebindingService m_Service;
        private readonly SynchronizationContext m_Context;

        /// <param name="deleteKey">The key UI/Delete starts on.</param>
        internal Rebinding(string deleteKey)
        {
            // With no context to post to, an await carries on as the key press completes the rebinding, inside the test.
            m_Context = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            m_Input = new InputTestFixture();
            m_Input.Setup();

            Keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Gamepad>();

            m_Actions = MakeActions(deleteKey);
            m_Actions.Enable();

            m_Map = new TestVariableMap().AddRecord(CoreVariables.INPUT_BINDINGS, MemoryBank.Settings, 0, RECORDS, new[]
            {
                new VariableRecordField(CoreVariables.INPUT_BINDING,      VariableWidth.Byte, CoreVariables.INPUT_BINDING_BYTES,      0),
                new VariableRecordField(CoreVariables.INPUT_BINDING_PATH, VariableWidth.Byte, CoreVariables.INPUT_BINDING_PATH_BYTES, 0)
            });

            m_Memory  = TestMemory.WithSettings(RECORDS * (CoreVariables.INPUT_BINDING_BYTES + CoreVariables.INPUT_BINDING_PATH_BYTES));
            m_Service = new InputRebindingService(new FixedActions(m_Actions), m_Memory, m_Map);

            Confirm = new RebindableControl(new[] { "Player/Confirm", "UI/Submit" }, null);
            Cancel  = new RebindableControl(new[] { "Player/Cancel", "UI/Cancel" }, null);
            Menu    = new RebindableControl(new[] { "Player/Menu" }, null);
            Delete  = new RebindableControl(new[] { "UI/Delete" }, null);
            Up      = new RebindableControl(new[] { "Player/Movement" }, "up");
        }

        internal Keyboard Keyboard { get; }

        internal IInputRebindingService Service => m_Service;

        internal RebindableControl[] All => new[] { Confirm, Cancel, Menu, Delete, Up };

        /// <summary>Rebinds the control in the scheme to whatever <paramref name="press" /> is, as the player would.</summary>
        internal RebindOutcome Rebind(RebindableControl control, string scheme, ButtonControl press)
        {
            Task<RebindOutcome> rebinding = m_Service.RebindAsync(control, scheme, All);

            m_Input.Press(press);

            for (int update = 0; update < 10 && !rebinding.IsCompleted; update++)
            {
                m_Input.currentTime += 0.2;
                InputSystem.Update();
            }

            m_Input.Release(press);

            return rebinding.IsCompleted ? rebinding.Result : (RebindOutcome)(-1);
        }

        /// <summary>Starts rebinding the control, and presses nothing.</summary>
        internal RebindOutcome Wait(RebindableControl control, string scheme)
        {
            Task<RebindOutcome> rebinding = m_Service.RebindAsync(control, scheme, All);

            m_Input.currentTime += WAITED_SECONDS;
            InputSystem.Update();

            return rebinding.IsCompleted ? rebinding.Result : (RebindOutcome)(-1);
        }

        /// <summary>The control <paramref name="action" />'s binding in the scheme is on now, a part's for a direction.</summary>
        internal string PathOf(string action, string scheme, string part = null)
        {
            return PathIn(m_Actions, action, scheme, part);
        }

        /// <summary>The same, on a later launch: the game's actions as they ship, with the kept bindings put on them.</summary>
        internal string PathAfterLaunch(string action, string scheme, string part = null)
        {
            InputActionAsset launched = InputActionAsset.FromJson(m_Actions.ToJson());

            try
            {
                IInputBindingsLoader loader = new InputRebindingService(new FixedActions(launched), m_Memory, m_Map);
                loader.Load();

                return PathIn(launched, action, scheme, part);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(launched);
            }
        }

        internal void Destroy()
        {
            m_Actions.Disable();
            UnityEngine.Object.DestroyImmediate(m_Actions);
            m_Input.TearDown();

            SynchronizationContext.SetSynchronizationContext(m_Context);
        }

        private static string PathIn(InputActionAsset actions, string actionPath, string scheme, string part)
        {
            InputAction action = actions.FindAction(actionPath);

            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                bool         isPart  = part == null ? !binding.isComposite && !binding.isPartOfComposite : binding.name == part;

                if (isPart && binding.groups == scheme)
                {
                    return binding.effectivePath;
                }
            }

            return null;
        }

        private static InputActionAsset MakeActions(string deleteKey)
        {
            InputActionAsset actions = ScriptableObject.CreateInstance<InputActionAsset>();

            actions.AddControlScheme(KEYBOARD).WithRequiredDevice("<Keyboard>").Done();
            actions.AddControlScheme(GAMEPAD).WithRequiredDevice("<Gamepad>").Done();

            InputActionMap player = actions.AddActionMap("Player");
            AddButton(player, "Confirm", "<Keyboard>/e", "<Gamepad>/buttonSouth");
            AddButton(player, "Cancel", "<Keyboard>/escape", "<Gamepad>/buttonEast");
            AddButton(player, "Menu", "<Keyboard>/q", "<Gamepad>/buttonNorth");

            player.AddAction("Movement", InputActionType.Value, expectedControlLayout: "Vector2")
                  .AddCompositeBinding("2DVector")
                  .With("up", "<Keyboard>/w", KEYBOARD)
                  .With("down", "<Keyboard>/s", KEYBOARD)
                  .With("left", "<Keyboard>/a", KEYBOARD)
                  .With("right", "<Keyboard>/d", KEYBOARD);

            InputActionMap ui = actions.AddActionMap("UI");
            AddButton(ui, "Submit", "<Keyboard>/e", "<Gamepad>/buttonSouth");
            AddButton(ui, "Cancel", "<Keyboard>/escape", "<Gamepad>/buttonEast");
            AddButton(ui, "Delete", deleteKey, "<Gamepad>/buttonWest");

            return actions;
        }

        private static void AddButton(InputActionMap map, string name, string key, string padButton)
        {
            InputAction action = map.AddAction(name, InputActionType.Button);

            action.AddBinding(key).WithGroup(KEYBOARD);
            action.AddBinding(padButton).WithGroup(GAMEPAD);
        }

        private sealed class FixedActions : IInputActionsProvider
        {
            private readonly InputActionAsset m_Actions;

            internal FixedActions(InputActionAsset actions)
            {
                m_Actions = actions;
            }

            InputActionAsset IInputActionsProvider.Actions => m_Actions;
        }
    }
}
