using NUnit.Framework;
using RPGFramework.Core.Input;
using RPGFramework.DI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input
{
    /// <summary>
    /// A direction and confirm pressed while a module's input is off — the field under the menu, or through a jump — and
    /// still held as it comes back on. The actions are enabled first, as the Input System enables a project's actions as
    /// play starts.
    /// </summary>
    internal sealed class WhenKeysAreHeldAsInputComesBackOn
    {
        private InputTestFixture      m_InputSystem;
        private InputActionAsset      m_Actions;
        private InputActionReference  m_Movement;
        private InputActionReference  m_Primary;
        private GameObject            m_Holder;
        private InputAdapter          m_Adapter;
        private RecordingInputContext m_Context;

        [SetUp]
        public void SetUp()
        {
            m_InputSystem = new InputTestFixture();
            m_InputSystem.Setup();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();

            m_Actions = ScriptableObject.CreateInstance<InputActionAsset>();

            InputActionMap map     = m_Actions.AddActionMap("Field");
            InputAction    move    = map.AddAction("Navigate", InputActionType.PassThrough);
            InputAction    confirm = map.AddAction("Confirm", InputActionType.Button, "<Keyboard>/enter");

            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");

            m_Actions.Enable();

            m_Movement = InputActionReference.Create(move);
            m_Primary  = InputActionReference.Create(confirm);
            m_Holder   = new GameObject("Input");
            m_Adapter  = m_Holder.AddComponent<InputAdapter>();

            SerializedObject adapter = new SerializedObject(m_Adapter);
            adapter.FindProperty("m_Movement").objectReferenceValue = m_Movement;
            adapter.FindProperty("m_Primary").objectReferenceValue  = m_Primary;
            adapter.ApplyModifiedPropertiesWithoutUndo();

            IInputRouter router    = new InputRouter();
            IDIContainer container = new DIContainer();

            m_Context = new RecordingInputContext(true);
            router.Push(m_Context);
            container.BindSingletonFromInstance(router);
            ((IDIResolver)container).InjectInto(m_Adapter);

            m_Adapter.Enable();
            m_Adapter.Disable();

            m_InputSystem.Press(keyboard.wKey);
            m_InputSystem.Press(keyboard.enterKey);

            m_Adapter.Enable();
            InputSystem.Update();
        }

        [TearDown]
        public void TearDown()
        {
            m_Adapter.Disable();

            Object.DestroyImmediate(m_Holder);
            Object.DestroyImmediate(m_Movement);
            Object.DestroyImmediate(m_Primary);
            Object.DestroyImmediate(m_Actions);

            m_InputSystem.TearDown();
        }

        [Test]
        public void ThenTheHeldDirectionIsItsMovement()
        {
            Assert.AreEqual(Vector2.up, m_Adapter.Movement);
        }

        [Test]
        public void ThenTheHeldConfirmDoesNotReachTheContext()
        {
            Assert.AreEqual(0, m_Context.Presses);
        }
    }
}
