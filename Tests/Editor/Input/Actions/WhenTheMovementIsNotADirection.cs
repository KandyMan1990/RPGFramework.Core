using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Input;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input.Actions
{
    /// <summary>A field moving with a button.</summary>
    internal sealed class WhenTheMovementIsNotADirection
    {
        private ActionsToCheck m_Actions;
        private List<string>   m_Problems;

        [SetUp]
        public void SetUp()
        {
            m_Actions = new ActionsToCheck();
            m_Problems = m_Actions.Check("Confirm", new ModuleReading(null, true));
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
            StringAssert.Contains("moves with 'Confirm', which is not a direction", m_Problems[0]);
        }
    }
}
