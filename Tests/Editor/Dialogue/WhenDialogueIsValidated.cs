using NUnit.Framework;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenDialogueIsValidated
    {
        [Test]
        public void ThenEveryPageIsRead()
        {
            string[] problems = Markup.Validate("Fine.{NewPage}{Nope}");

            Assert.AreEqual(1, problems.Length, string.Join("; ", problems));
            StringAssert.Contains("Nope", problems[0]);
        }

        [Test]
        public void ThenASpeakerInBracketsIsNotMarkup()
        {
            CollectionAssert.IsEmpty(Markup.Validate("[Guard] Halt!"));
        }
    }
}
