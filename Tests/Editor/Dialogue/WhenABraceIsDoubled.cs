using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenABraceIsDoubled
    {
        [Test]
        public void ThenItIsALiteralBrace()
        {
            DialogueText text = Markup.Parse("A {{ brace", out List<string> problems);

            Assert.AreEqual("A { brace", text.Text);
            CollectionAssert.IsEmpty(problems);
        }
    }
}
