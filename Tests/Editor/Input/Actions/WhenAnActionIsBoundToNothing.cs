using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Input;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input.Actions
{
    /// <summary>A module reading an action the map has, with no binding, as a new action starts.</summary>
    internal sealed class WhenAnActionIsBoundToNothing
    {
        private ActionsToCheck m_Actions;
        private List<string>   m_Problems;

        [SetUp]
        public void SetUp()
        {
            m_Actions = new ActionsToCheck();
            m_Actions.Player.AddAction("Menu", InputActionType.Button);

            m_Problems = m_Actions.Check("Movement", new ModuleReading(null, false, ModuleReading.Button("Menu")));
        }

        [TearDown]
        public void TearDown()
        {
            m_Actions.Destroy();
        }

        [Test]
        public void ThenItIsReported()
        {
            Assert.AreEqual(1, m_Problems.Count);
            StringAssert.Contains("'Player' / 'Menu' is bound to nothing", m_Problems[0]);
        }
    }
}
