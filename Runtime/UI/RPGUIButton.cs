using System;
using UnityEngine.UIElements;

namespace RPGFramework.Core.UI
{
    /// <summary>
    /// A button the cursor moves between: highlighted while focused, focused as the pointer enters it, and chosen by a
    /// click or a submit. Its neighbours say where each direction takes the cursor in a <see cref="SelectionScope" />.
    /// </summary>
    [UxmlElement]
    public partial class RPGUIButton : VisualElement
    {
        /// <summary>A click on the button, or a submit while it has focus.</summary>
        public event Action Chosen;

        [UxmlAttribute]
        public string text
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        /// <summary>Where up takes the cursor; none leaves the press to the button.</summary>
        public RPGUIButton Up { get; set; }

        /// <summary>Where down takes the cursor; none leaves the press to the button.</summary>
        public RPGUIButton Down { get; set; }

        /// <summary>Where left takes the cursor; none leaves the press to the button, as a slider wants.</summary>
        public RPGUIButton Left { get; set; }

        /// <summary>Where right takes the cursor; none leaves the press to the button, as a slider wants.</summary>
        public RPGUIButton Right { get; set; }

        internal Label Label => m_Label;

        private readonly Image m_Icon;
        private readonly Label m_Label;

        private bool m_IsHoveringOrFocused;

        public RPGUIButton()
        {
            AddToClassList("rpg-ui-button");

            m_Icon = new Image();
            m_Icon.AddToClassList("rpg-ui-button-icon");
            m_Icon.style.opacity = 0f;
            m_Icon.focusable     = false;

            m_Label      = new Label();
            m_Label.text = "New Text";
            m_Label.AddToClassList("rpg-ui-label");
            m_Label.focusable = false;

            Add(m_Icon);
            Add(m_Label);

            pickingMode = PickingMode.Position;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<NavigationSubmitEvent>(OnSubmit);
        }

        /// <summary>
        /// Links the buttons top to bottom, each one's <see cref="Down" /> the next and <see cref="Up" /> the one
        /// before; with <paramref name="wrap" /> the last leads on to the first and the first back to the last.
        /// </summary>
        public static void LinkColumn(RPGUIButton[] buttons, bool wrap)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Up   = Previous(buttons, i, wrap);
                buttons[i].Down = Next(buttons, i, wrap);
            }
        }

        /// <summary>
        /// Links the buttons left to right, each one's <see cref="Right" /> the next and <see cref="Left" /> the one
        /// before; with <paramref name="wrap" /> the last leads on to the first and the first back to the last.
        /// </summary>
        public static void LinkRow(RPGUIButton[] buttons, bool wrap)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Left  = Previous(buttons, i, wrap);
                buttons[i].Right = Next(buttons, i, wrap);
            }
        }

        private static RPGUIButton Previous(RPGUIButton[] buttons, int index, bool wrap)
        {
            int         previous = index > 0 ? index - 1 : wrap ? buttons.Length - 1 : -1;
            RPGUIButton button   = previous >= 0 && previous != index ? buttons[previous] : null;

            return button;
        }

        private static RPGUIButton Next(RPGUIButton[] buttons, int index, bool wrap)
        {
            int         next   = index < buttons.Length - 1 ? index + 1 : wrap ? 0 : -1;
            RPGUIButton button = next >= 0 && next != index ? buttons[next] : null;

            return button;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            focusable = true;
            UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            if (evt.target == this)
            {
                ShowHighlight(true);
            }
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            if (evt.target == this)
            {
                ShowHighlight(false);
            }
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            Focus();
        }

        private void OnClick(ClickEvent evt)
        {
            Chosen?.Invoke();
        }

        private void OnSubmit(NavigationSubmitEvent evt)
        {
            Chosen?.Invoke();
        }

        private void ShowHighlight(bool show)
        {
            if (show == m_IsHoveringOrFocused)
            {
                return;
            }

            m_IsHoveringOrFocused = show;
            m_Icon.style.opacity  = show ? 1f : 0f;
        }
    }
}