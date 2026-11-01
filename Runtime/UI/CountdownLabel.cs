using RPGFramework.Core.Store;
using UnityEngine.UIElements;

namespace RPGFramework.Core.UI
{
    /// <summary>
    /// The countdown as minutes and seconds while it is shown, where a script put it, its colon blinking once a second,
    /// and hidden otherwise: what the field and a battle carrying it put on screen, in the same place. The module adds it
    /// and ticks it; a game restyles it through its <c>rpg-countdown</c> class, which Core's shared styles size.
    /// </summary>
    public sealed class CountdownLabel : Label
    {
        private const string USS_CLASS = "rpg-countdown";

        private readonly ICountdownStore m_Countdown;
        private readonly ClockBlink      m_Blink;

        public CountdownLabel(ICountdownStore countdown)
        {
            m_Countdown = countdown;
            m_Blink     = new ClockBlink();

            name           = "Countdown";
            pickingMode    = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.display  = DisplayStyle.None;

            AddToClassList("rpg-ui-label");
            AddToClassList(USS_CLASS);
        }

        /// <summary>Shows the seconds left, or hides it while the countdown is not shown.</summary>
        public void Tick(float deltaTime)
        {
            bool shown = m_Countdown.IsShown;

            style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;

            if (!shown)
            {
                return;
            }

            uint seconds = m_Countdown.Seconds;

            style.left = m_Countdown.X;
            style.top  = m_Countdown.Y;
            text       = ClockText.Countdown(seconds, seconds == 0 || m_Blink.Tick(seconds, deltaTime));
        }
    }
}
