using NUnit.Framework;
using RPGFramework.Core.UI;

namespace RPGFramework.Core.Tests.UI
{
    internal sealed class WhenAClockIsShown
    {
        [Test]
        public void ThenThePlayTimeIsHoursAndMinutesPastAHundred()
        {
            Assert.AreEqual("123:04", ClockText.PlayTime(123 * 3600 + 4 * 60 + 59, true));
        }

        [Test]
        public void ThenTheCountdownIsTwoDigitsOfMinutesAndOfSeconds()
        {
            Assert.AreEqual("04:05", ClockText.Countdown(4 * 60 + 5, true));
        }

        [Test]
        public void ThenItsColonIsLitAsTheSecondsChange()
        {
            ClockBlink blink = new ClockBlink();

            Assert.IsTrue(blink.Tick(10, 0f));
        }

        [Test]
        public void ThenItsColonDimsHalfASecondLater()
        {
            ClockBlink blink = new ClockBlink();

            blink.Tick(10, 0f);

            Assert.IsFalse(blink.Tick(10, 0.6f));
        }

        [Test]
        public void ThenADimColonIsDrawnAtTwoThirds()
        {
            Assert.AreEqual("04<alpha=#AA>:</alpha>05", ClockText.Countdown(4 * 60 + 5, false));
        }
    }
}
