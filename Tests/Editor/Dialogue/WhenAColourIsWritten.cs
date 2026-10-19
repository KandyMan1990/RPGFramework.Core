using System.Collections.Generic;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using UnityEngine;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenAColourIsWritten
    {
        [Test]
        public void ThenAHexColourColoursItsSpan()
        {
            DialogueText text = Markup.Parse("{Colour #2CE2DD}Flamethrower{/Colour}", out List<string> problems);

            Assert.IsTrue(text.TryGetColour(0, out Color colour));
            Assert.IsTrue(Markup.Same(new Color32(0x2C, 0xE2, 0xDD, 0xFF), colour), colour.ToString());
            CollectionAssert.IsEmpty(problems);
        }

        [Test]
        public void ThenAColoursNameIsTaken()
        {
            DialogueText text = Markup.Parse("{Colour yellow}sword{/Colour}", out List<string> problems);

            Assert.IsTrue(text.TryGetColour(0, out Color _));
            CollectionAssert.IsEmpty(problems);
        }

        [Test]
        public void ThenOneThatIsNotAColourIsAProblem()
        {
            Markup.Parse("{Colour notacolour}x{/Colour}", out List<string> problems);

            Assert.AreEqual(1, problems.Count, string.Join("; ", problems));
        }
    }
}
