using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using UnityEngine;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenSpansAreNested
    {
        [Test]
        public void ThenABlinkInsideAStyleKeepsTheStylesColour()
        {
            DialogueText text = Markup.Parse("{Location}the {Blink}old{/Blink} mill{/Location}", out List<string> problems);

            Assert.IsTrue(text.TryGetColour(4, out Color colour), string.Join("; ", problems));
            Assert.IsTrue(Markup.Same(Color.yellow, colour), colour.ToString());
            Assert.IsTrue(text.IsBlinking(4));
            Assert.IsFalse(text.IsBlinking(0));
        }

        [Test]
        public void ThenTheInnermostColourWins()
        {
            DialogueText text = Markup.Parse("{Location}a {Colour #FF0000}b{/Colour} c{/Location}", out List<string> _);

            Assert.IsTrue(text.TryGetColour(2, out Color inner));
            Assert.IsTrue(Markup.Same(Color.red, inner), inner.ToString());
            Assert.IsTrue(text.TryGetColour(0, out Color outer));
            Assert.IsTrue(Markup.Same(Color.yellow, outer), outer.ToString());
        }
    }
}
