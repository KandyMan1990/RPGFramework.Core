using UnityEngine.UIElements;

namespace RPGFramework.Core.UI
{
    [UxmlElement]
    public partial class RPGUIButton : VisualElement
    {
        [UxmlAttribute]
        public string text
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

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