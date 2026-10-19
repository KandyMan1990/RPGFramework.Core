using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenAMessageVariableIsShown
    {
        [Test]
        public void ThenItShowsTheVariablesValue()
        {
            DialogueText text = Markup.Parse("You have {Var 0} gold and {Var 7} potions.", out List<string> _, new[] { 1500, 0, 0, 0, 0, 0, 0, 3 });

            Assert.AreEqual("You have 1500 gold and 3 potions.", text.Text);
        }

        [Test]
        public void ThenOneWithNoValueShowsZero()
        {
            DialogueText text = Markup.Parse("{Var 2}", out List<string> problems);

            Assert.AreEqual("0", text.Text);
            CollectionAssert.IsEmpty(problems);
        }

        [Test]
        public void ThenASlotPastTheLastIsAProblem()
        {
            Markup.Parse("{Var 8}", out List<string> problems);

            Assert.AreEqual(1, problems.Count, string.Join("; ", problems));
        }
    }
}
