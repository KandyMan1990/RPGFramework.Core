using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Input;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input.Actions
{
    /// <summary>A module pressing the action the player moves with.</summary>
    internal sealed class WhenAnActionIsNotTheKindItIsReadAs
    {
        private ActionsToCheck m_Actions;
        private List<string>   m_Problems;

        [SetUp]
        public void SetUp()
        {
            m_Actions = new ActionsToCheck();
            m_Problems = m_Actions.Check("Movement", new ModuleReading(null, false, ModuleReading.Button("Movement")));
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
            StringAssert.Contains("'Player' / 'Movement' is not a Button action", m_Problems[0]);
        }
    }
}
