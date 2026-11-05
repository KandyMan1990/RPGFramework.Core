using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Input;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input.Actions
{
    /// <summary>A field that moves and confirms, its adapter routing a map holding both, bound.</summary>
    internal sealed class WhenEveryActionAModuleReadsIsInItsMap
    {
        private ActionsToCheck m_Actions;
        private List<string>   m_Problems;

        [SetUp]
        public void SetUp()
        {
            m_Actions = new ActionsToCheck();
            m_Problems = m_Actions.Check("Movement", new ModuleReading("Player", true, ModuleReading.Button("Confirm")));
        }

        [TearDown]
        public void TearDown()
        {
            m_Actions.Destroy();
        }

        [Test]
        public void ThenNothingIsReported()
        {
            CollectionAssert.IsEmpty(m_Problems);
        }
    }
}
