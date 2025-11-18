using System;
using UnityEngine.UIElements;

namespace RPGFramework.Core.UI
{
    [UxmlElement]
    public partial class RPGUIButton : VisualElement
    {
        public static event Action OnClicked;
        public static event Action OnHighlighted;

        [UxmlAttribute]
        public string text
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        private readonly Image m_Icon;
        private readonly Label m_Label;

        private bool m_HoverOrFocus;

        public RPGUIButton()
        {
            AddToClassList("rpg-ui-button");

            m_Icon = new Image();
            m_Icon.AddToClassList("rpg-ui-button-icon");
            ShowIcon(false);

            m_Label      = new Label();
            m_Label.text = "New Text";
            m_Label.AddToClassList("rpg-ui-label");

            Add(m_Icon);
            Add(m_Label);

            RegisterCallback<AttachToPanelEvent>(evt =>
                                                 {
                                                     focusable   = true;
                                                     pickingMode = PickingMode.Position;
                                                 });

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<BlurEvent>(OnBlur);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<NavigationSubmitEvent>(OnSubmit);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            VisualElement root = panel?.visualTree;

            root?.RegisterCallback<FocusInEvent>(OnAnyFocus, TrickleDown.TrickleDown);
            root?.RegisterCallback<PointerEnterEvent>(OnAnyHover, TrickleDown.TrickleDown);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            VisualElement root = panel?.visualTree;

            root?.UnregisterCallback<FocusInEvent>(OnAnyFocus, TrickleDown.TrickleDown);
            root?.UnregisterCallback<PointerEnterEvent>(OnAnyHover, TrickleDown.TrickleDown);
        }

        private void OnAnyFocus(FocusInEvent evt)
        {
            bool showIcon = evt.target == this;

            ShowIcon(showIcon);
        }

        private void OnAnyHover(PointerEnterEvent evt)
        {
            bool showIcon = evt.target == m_Label;

            ShowIcon(showIcon);
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            RaiseHighlightedEvent();
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            ResetHighlightedEvent();
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            RaiseHighlightedEvent();
        }

        private void OnBlur(BlurEvent evt)
        {
            ResetHighlightedEvent();
        }

        private void RaiseHighlightedEvent()
        {
            if (m_HoverOrFocus)
            {
                return;
            }
            m_HoverOrFocus = true;

            OnHighlighted?.Invoke();
        }

        private void ResetHighlightedEvent()
        {
            m_HoverOrFocus = false;
        }

        private void OnSubmit(NavigationSubmitEvent evt)
        {
            if (focusController.focusedElement == this)
                RaiseHighlightedEvent();

            OnClicked?.Invoke();
        }

        private void OnClick(ClickEvent evt)
        {
            if (focusController.focusedElement == this)
                RaiseHighlightedEvent();

            OnClicked?.Invoke();
        }

        private void ShowIcon(bool show)
        {
            m_Icon.style.opacity = show ? 1f : 0f;
        }
    }
}