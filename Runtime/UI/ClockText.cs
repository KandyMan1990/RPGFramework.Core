namespace RPGFramework.Core.UI
{
    /// <summary>
    /// A clock's text: the play time as hours and minutes, the hours never capped, as a long game passes a hundred; the
    /// countdown as minutes and seconds, two digits each. An unlit colon is drawn at two thirds, through the label's rich
    /// text.
    /// </summary>
    public static class ClockText
    {
        private const string UNLIT_COLON = "<alpha=#AA>:</alpha>";

        /// <summary>Hours and minutes of <paramref name="seconds" />.</summary>
        public static string PlayTime(uint seconds, bool colonLit) => $"{seconds / 3600}{Colon(colonLit)}{seconds / 60 % 60:00}";

        /// <summary>Minutes and seconds of <paramref name="seconds" />, as 05:00.</summary>
        public static string Countdown(uint seconds, bool colonLit) => $"{seconds / 60:00}{Colon(colonLit)}{seconds % 60:00}";

        private static string Colon(bool lit) => lit ? ":" : UNLIT_COLON;
    }

    /// <summary>
    /// A clock's colon blinking once a second: lit for the half second after the seconds shown change, unlit for the
    /// rest. A countdown at zero, which has stopped, keeps it lit.
    /// </summary>
    public sealed class ClockBlink
    {
        private const float LIT_SECONDS = 0.5f;

        private uint  m_Shown;
        private float m_SinceChange;

        public ClockBlink()
        {
            m_Shown       = uint.MaxValue;
            m_SinceChange = 0f;
        }

        /// <summary>Whether the colon is lit this frame, the clock showing <paramref name="seconds" />.</summary>
        public bool Tick(uint seconds, float deltaTime)
        {
            if (seconds != m_Shown)
            {
                m_Shown       = seconds;
                m_SinceChange = 0f;
            }
            else
            {
                m_SinceChange += deltaTime;
            }

            return m_SinceChange < LIT_SECONDS;
        }
    }
}
