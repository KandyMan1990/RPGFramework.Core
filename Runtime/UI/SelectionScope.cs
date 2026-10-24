using System;
using UnityEngine.UIElements;

namespace RPGFramework.Core.UI
{
    /// <summary>
    /// A part of the UI the player moves a cursor through — a menu, a question's answers, a battle's commands. The
    /// cursor moves only by its <see cref="RPGUIButton" />s' neighbours, each move the player makes is reported for the
    /// logic behind it to sound, and while its input is off nothing in it reacts to keys, pads or the pointer. It starts
    /// off.
    /// </summary>
    public sealed class SelectionScope
    {
        /// <summary>
        /// The player moved the cursor to another button, by a direction or the pointer. Focus the code sets is not a
        /// move.
        /// </summary>
        public event Action Moved;

        private readonly VisualElement m_Root;

        private bool m_InputEnabled;

        // UI Toolkit moves focus by itself — along its own focus ring on a navigation key, to a button the pointer
        // enters or presses — so every event is taken at the root as it trickles down, before it reaches a button.
        public SelectionScope(VisualElement root)
        {
            m_Root         = root;
            m_InputEnabled = false;

            root.RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationSubmitEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationCancelEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerEnterEvent>(OnPointerEnter, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerDownEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
            root.RegisterCallback<ClickEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
            root.RegisterCallback<WheelEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
        }

        /// <summary>Starts or stops the scope reacting to the player.</summary>
        public void SetInputEnabled(bool enabled)
        {
            m_InputEnabled = enabled;
        }

        // A direction with no neighbour passes on to the element, as a slider's row takes left and right, but UI
        // Toolkit's own focus ring never runs.
        private void OnNavigationMove(NavigationMoveEvent evt)
        {
            if (!m_InputEnabled)
            {
                Stop(evt);
                return;
            }

            m_Root.focusController.IgnoreEvent(evt);

            RPGUIButton button = FindButton(evt.target as VisualElement);
            RPGUIButton next   = button == null ? null : Neighbour(button, evt.direction);

            if (next == null)
            {
                return;
            }

            next.Focus();
            evt.StopImmediatePropagation();

            Moved?.Invoke();
        }

        // A button focuses itself as the pointer enters it, after this has seen the event on its way down.
        private void OnPointerEnter(PointerEnterEvent evt)
        {
            if (!m_InputEnabled)
            {
                Stop(evt);
                return;
            }

            RPGUIButton button = FindButton(evt.target as VisualElement);

            if (button != null && button != m_Root.focusController.focusedElement)
            {
                Moved?.Invoke();
            }
        }

        private void StopIfInputDisabled(EventBase evt)
        {
            if (!m_InputEnabled)
            {
                Stop(evt);
            }
        }

        private void Stop(EventBase evt)
        {
            evt.StopImmediatePropagation();
            m_Root.focusController.IgnoreEvent(evt);
        }

        private RPGUIButton FindButton(VisualElement element)
        {
            while (element != null && element != m_Root && !(element is RPGUIButton))
            {
                element = element.parent;
            }

            RPGUIButton button = element as RPGUIButton;

            return button;
        }

        private static RPGUIButton Neighbour(RPGUIButton button, NavigationMoveEvent.Direction direction)
        {
            RPGUIButton neighbour = direction switch
                                    {
                                        NavigationMoveEvent.Direction.Up    => button.Up,
                                        NavigationMoveEvent.Direction.Down  => button.Down,
                                        NavigationMoveEvent.Direction.Left  => button.Left,
                                        NavigationMoveEvent.Direction.Right => button.Right,
                                        _                                   => null
                                    };

            return neighbour;
        }
    }
}