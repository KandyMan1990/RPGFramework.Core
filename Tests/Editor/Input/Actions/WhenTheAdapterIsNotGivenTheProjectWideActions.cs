using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Tests.Input.Actions
{
    /// <summary>An adapter given actions of its own, which UI Toolkit does not read.</summary>
    internal sealed class WhenTheAdapterIsNotGivenTheProjectWideActions
    {
        private ActionsToCheck   m_Actions;
        private InputActionAsset m_ProjectWide;
        private List<string>     m_Problems;

        [SetUp]
        public void SetUp()
        {
            m_Actions     = new ActionsToCheck();
            m_ProjectWide = ScriptableObject.CreateInstance<InputActionAsset>();
            m_ProjectWide.name = "Project Actions";

            m_Problems = m_Actions.CheckAgainst(m_ProjectWide, "Movement");
        }

        [TearDown]
        public void TearDown()
        {
            m_Actions.Destroy();
            Object.DestroyImmediate(m_ProjectWide);
        }

        [Test]
        public void ThenItIsReported()
        {
            Assert.AreEqual(1, m_Problems.Count);
            StringAssert.Contains("not the project-wide actions 'Project Actions'", m_Problems[0]);
        }
    }
}
