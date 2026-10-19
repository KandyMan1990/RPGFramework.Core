using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using UnityEngine;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenAStyleIsWritten
    {
        private DialogueText m_Text;

        [SetUp]
        public void SetUp()
        {
            m_Text = Markup.Parse("Go to the {Location}secret cave{/Location} now.", out List<string> _);
        }

        [Test]
        public void ThenItCoversItsText()
        {
            Assert.AreEqual("Go to the secret cave now.", m_Text.Text);
            Assert.IsFalse(m_Text.TryGetColour(9, out Color _));
            Assert.IsTrue(m_Text.TryGetColour(10, out Color _));
            Assert.IsTrue(m_Text.TryGetColour(20, out Color _));
            Assert.IsFalse(m_Text.TryGetColour(21, out Color _));
        }

        [Test]
        public void ThenItTakesItsColour()
        {
            Assert.IsTrue(m_Text.TryGetColour(10, out Color colour));
            Assert.IsTrue(Markup.Same(Color.yellow, colour), colour.ToString());
        }

        [Test]
        public void ThenOneThatDoesNotBlinkDoesNot()
        {
            Assert.IsFalse(m_Text.IsBlinking(12));
        }

        [Test]
        public void ThenOneThatBlinksDoes()
        {
            DialogueText task = Markup.Parse("New {Task}objective{/Task}", out List<string> _);

            Assert.IsTrue(task.IsBlinking(4));
            Assert.IsFalse(task.IsBlinking(3));
        }

        [Test]
        public void ThenOneNotDeclaredIsAProblem()
        {
            Markup.Parse("Go to {Nowhere}here{/Nowhere}", out List<string> problems);

            Assert.IsTrue(problems.Exists(problem => problem.Contains("Nowhere")), string.Join("; ", problems));
        }
    }
}
