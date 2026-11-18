using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Tests.UI
{
    internal sealed class WhenAScopesInputIsOff
    {
        private ScopePanel m_Panel;
        private int        m_Moves;
        private int        m_FirstChosen;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            m_Panel       = new ScopePanel();
            m_Moves       = 0;
            m_FirstChosen = 0;

            yield return m_Panel.LayOut();

            m_Panel.Scope.Moved += () => m_Moves++;
            m_Panel.Scope.RegisterChosen(m_Panel.Elements[0], () => m_FirstChosen++);
            m_Panel.Elements[0].Focus();
        }

        [TearDown]
        public void TearDown()
        {
            m_Panel.Close();
        }

        [Test]
        public void ThenADirectionMovesNothing()
        {
            m_Panel.Move(m_Panel.Elements[0], NavigationMoveEvent.Direction.Down);

            Assert.AreSame(m_Panel.Elements[0], m_Panel.Focused);
        }

        [Test]
        public void ThenNoMoveIsReported()
        {
            m_Panel.Move(m_Panel.Elements[0], NavigationMoveEvent.Direction.Down);

            Assert.AreEqual(0, m_Moves);
        }

        [Test]
        public void ThenASubmitChoosesNothing()
        {
            m_Panel.Submit(m_Panel.Elements[0]);

            Assert.AreEqual(0, m_FirstChosen);
        }
    }
}
