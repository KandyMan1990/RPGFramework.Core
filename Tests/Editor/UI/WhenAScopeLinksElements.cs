using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Tests.UI
{
    internal sealed class WhenAScopeLinksElements
    {
        private ScopePanel m_Panel;
        private int        m_Moves;
        private int        m_SecondChosen;
        private int        m_PassedOn;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            m_Panel        = new ScopePanel();
            m_Moves        = 0;
            m_SecondChosen = 0;
            m_PassedOn     = 0;

            yield return m_Panel.LayOut();

            m_Panel.Scope.Moved += () => m_Moves++;
            m_Panel.Scope.RegisterChosen(m_Panel.Elements[1], () => m_SecondChosen++);
            m_Panel.Elements[1].RegisterCallback<NavigationMoveEvent>(_ => m_PassedOn++);
            m_Panel.Scope.SetInputEnabled(true);
            m_Panel.Elements[0].Focus();
        }

        [TearDown]
        public void TearDown()
        {
            m_Panel.Close();
        }

        [Test]
        public void ThenEachTakesTheCursor()
        {
            Assert.IsTrue(m_Panel.Elements[2].focusable);
        }

        [Test]
        public void ThenEachIsClassedForTheCursorToBeDrawn()
        {
            Assert.IsTrue(m_Panel.Elements[2].ClassListContains("rpg-selectable"));
        }

        [Test]
        public void ThenDownMovesTheCursorToTheNext()
        {
            m_Panel.Move(m_Panel.Elements[0], NavigationMoveEvent.Direction.Down);

            Assert.AreSame(m_Panel.Elements[1], m_Panel.Focused);
        }

        [Test]
        public void ThenUpFromTheFirstWrapsToTheLast()
        {
            m_Panel.Move(m_Panel.Elements[0], NavigationMoveEvent.Direction.Up);

            Assert.AreSame(m_Panel.Elements[2], m_Panel.Focused);
        }

        [Test]
        public void ThenTheMoveIsReported()
        {
            m_Panel.Move(m_Panel.Elements[0], NavigationMoveEvent.Direction.Down);

            Assert.AreEqual(1, m_Moves);
        }

        [Test]
        public void ThenFocusTheCodeSetsIsNoMove()
        {
            m_Panel.Elements[2].Focus();

            Assert.AreEqual(0, m_Moves);
        }

        [Test]
        public void ThenADirectionWithNoNeighbourPassesToTheElement()
        {
            m_Panel.Elements[1].Focus();
            m_Panel.Move(m_Panel.Elements[1], NavigationMoveEvent.Direction.Left);

            Assert.AreEqual(1, m_PassedOn);
        }

        [Test]
        public void ThenASubmitChoosesTheElement()
        {
            m_Panel.Elements[1].Focus();
            m_Panel.Submit(m_Panel.Elements[1]);

            Assert.AreEqual(1, m_SecondChosen);
        }

        [Test]
        public void ThenASubmitOnAnotherChoosesNotThisOne()
        {
            m_Panel.Submit(m_Panel.Elements[0]);

            Assert.AreEqual(0, m_SecondChosen);
        }

        [Test]
        public void ThenANeighbourSetLaterIsWhereTheCursorGoes()
        {
            m_Panel.Scope.SetNeighbour(m_Panel.Elements[0], NavigationMoveEvent.Direction.Right, m_Panel.Elements[2]);
            m_Panel.Move(m_Panel.Elements[0], NavigationMoveEvent.Direction.Right);

            Assert.AreSame(m_Panel.Elements[2], m_Panel.Focused);
        }
    }
}
