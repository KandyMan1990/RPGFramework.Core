using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Input;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input.Actions
{
    /// <summary>A module opening its menu with an action the map does not have.</summary>
    internal sealed class WhenAnActionAModuleReadsIsMissing
    {
        private ActionsToCheck m_Actions;
        private List<string>   m_Problems;

        [SetUp]
        public void SetUp()
        {
            m_Actions = new ActionsToCheck();
            m_Problems = m_Actions.Check("Movement", new ModuleReading(null, false, ModuleReading.Button("Confirm"), ModuleReading.Button("Menu")));
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
            StringAssert.Contains("'Player' has no 'Menu' action", m_Problems[0]);
        }

        [Test]
        public void ThenItIsOfferedToBeAdded()
        {
            List<RequiredAction> missing = InputActionsCheck.Missing(m_Actions.Player, new IRequiredActions[] { new ModuleReading(null, false, ModuleReading.Button("Confirm"), ModuleReading.Button("Menu")) });

            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual("Menu", missing[0].Name);
        }
    }
}
