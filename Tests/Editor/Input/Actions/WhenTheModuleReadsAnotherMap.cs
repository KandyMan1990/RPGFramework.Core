using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Input;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input.Actions
{
    /// <summary>A menu, which reads the UI map, with its adapter routing the player's.</summary>
    internal sealed class WhenTheModuleReadsAnotherMap
    {
        private ActionsToCheck m_Actions;
        private List<string>   m_Problems;

        [SetUp]
        public void SetUp()
        {
            m_Actions = new ActionsToCheck();
            m_Problems = m_Actions.Check(string.Empty, new ModuleReading("UI", false));
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
            StringAssert.Contains("routes the map 'Player', but", m_Problems[0]);
        }
    }
}
