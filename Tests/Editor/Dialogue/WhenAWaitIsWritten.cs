using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenAWaitIsWritten
    {
        [Test]
        public void ThenTheTypingPausesWhereItIsWritten()
        {
            DialogueText text = Markup.Parse("Wait.{Wait 1.5} Now.", out List<string> _);

            Assert.AreEqual("Wait. Now.", text.Text);
            Assert.AreEqual(1, text.Pauses.Count);
            Assert.AreEqual(5, text.Pauses[0].Index);
            Assert.AreEqual(1.5f, text.Pauses[0].Seconds);
        }

        [Test]
        public void ThenOneWithoutANumberIsAProblem()
        {
            Markup.Parse("{Wait soon}", out List<string> problems);

            Assert.AreEqual(1, problems.Count);
        }
    }
}
