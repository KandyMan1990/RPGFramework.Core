using System.Collections.Generic;
using NUnit.Framework;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenMarkupIsMalformed
    {
        [Test]
        public void ThenAnUnclosedSpanIsAProblem()
        {
            Markup.Parse("{Location}never closed", out List<string> problems);

            Assert.AreEqual(1, problems.Count, string.Join("; ", problems));
            StringAssert.Contains("never closed", problems[0]);
        }

        [Test]
        public void ThenSpansClosedOutOfOrderAreAProblem()
        {
            Markup.Parse("{Location}a{Blink}b{/Location}{/Blink}", out List<string> problems);

            CollectionAssert.IsNotEmpty(problems);
        }

        [Test]
        public void ThenAnUnclosedBraceIsAProblem()
        {
            Markup.Parse("A { brace", out List<string> problems);

            Assert.AreEqual(1, problems.Count, string.Join("; ", problems));
        }
    }
}
