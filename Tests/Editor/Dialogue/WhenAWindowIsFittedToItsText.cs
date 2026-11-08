using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using UnityEngine;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>A window on a screen 1920 by 1080, in UI units.</summary>
    internal sealed class WhenAWindowIsFittedToItsText
    {
        private static readonly Vector2 m_Screen = new Vector2(1920f, 1080f);

        [Test]
        public void ThenItGoesWhereItWasPut()
        {
            Assert.AreEqual(new RectInt(100, 200, 300, 150), Fit(100, 200, 300f, 150f));
        }

        [Test]
        public void ThenItsSizeIsRoundedUp()
        {
            RectInt rect = Fit(100, 200, 300.2f, 150.2f);

            Assert.AreEqual(301, rect.width);
            Assert.AreEqual(151, rect.height);
        }

        [Test]
        public void ThenItIsMovedBackFromTheRightEdge()
        {
            Assert.AreEqual(1920 - DialogueWindowFit.SCREEN_MARGIN - 300, Fit(1800, 200, 300f, 150f).x);
        }

        [Test]
        public void ThenItIsMovedBackFromTheBottomEdge()
        {
            Assert.AreEqual(1080 - DialogueWindowFit.SCREEN_MARGIN - 150, Fit(100, 1000, 300f, 150f).y);
        }

        [Test]
        public void ThenItKeepsToTheMarginAtTheTopLeft()
        {
            RectInt rect = Fit(0, 0, 300f, 150f);

            Assert.AreEqual(DialogueWindowFit.SCREEN_MARGIN, rect.x);
            Assert.AreEqual(DialogueWindowFit.SCREEN_MARGIN, rect.y);
        }

        [Test]
        public void ThenOneTooTallForTheScreenStartsAtTheTop()
        {
            Assert.AreEqual(DialogueWindowFit.SCREEN_MARGIN, Fit(100, 200, 300f, 2000f).y);
        }

        [Test]
        public void ThenItsTextWrapsAtTheScreensEdge()
        {
            Assert.AreEqual(1920f - 2 * DialogueWindowFit.SCREEN_MARGIN - 32f, DialogueWindowFit.WidestText(1920f, 32f));
        }

        private static RectInt Fit(int x, int y, float width, float height)
        {
            return DialogueWindowFit.Rect(new Vector2Int(x, y), new Vector2(width, height), m_Screen);
        }
    }
}
