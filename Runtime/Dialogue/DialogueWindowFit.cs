using Unity.Mathematics;
using UnityEngine;

namespace RPGFramework.Core.Dialogue
{
    /// <summary>Where a window sized to its text goes, kept on the screen.</summary>
    internal static class DialogueWindowFit
    {
        /// <summary>How far a fitted window stays from the screen's edges, in UI units.</summary>
        internal const int SCREEN_MARGIN = 48;

        /// <summary>The widest a fitted window's text can be before it wraps.</summary>
        internal static float WidestText(float screenWidth, float frameWidth)
        {
            float widest = screenWidth - 2 * SCREEN_MARGIN - frameWidth;

            return widest;
        }

        /// <summary>
        /// The window at a point, moved back from the right and bottom edges and then, should it be too big for the
        /// screen, from the left and top ones.
        /// </summary>
        internal static RectInt Rect(Vector2Int position, Vector2 size, Vector2 screen)
        {
            int width  = (int)math.ceil(size.x);
            int height = (int)math.ceil(size.y);
            int x      = math.max(SCREEN_MARGIN, math.min(position.x, (int)screen.x - SCREEN_MARGIN - width));
            int y      = math.max(SCREEN_MARGIN, math.min(position.y, (int)screen.y - SCREEN_MARGIN - height));

            return new RectInt(x, y, width, height);
        }
    }
}
