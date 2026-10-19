using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenPlainTextIsParsed
    {
        private DialogueText m_Text;
        private List<string> m_Problems;

        [SetUp]
        public void SetUp()
        {
            m_Text = Markup.Parse("Hello there.", out m_Problems);
        }

        [Test]
        public void ThenItIsUnchanged()
        {
            Assert.AreEqual("Hello there.", m_Text.Text);
        }

        [Test]
        public void ThenThereIsNoProblem()
        {
            CollectionAssert.IsEmpty(m_Problems);
        }
    }
}
