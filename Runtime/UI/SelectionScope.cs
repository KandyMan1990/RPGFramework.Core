using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace RPGFramework.Core.UI
{
    /// <summary>
    /// A part of the UI the player moves a cursor through — a menu, a question's answers, a battle's commands. Any
    /// element it links can take the cursor: it moves only by the neighbours each is given, an element takes it as the
    /// pointer enters, and a click or a submit chooses it. Each move the player makes is reported for the logic behind
    /// it to sound, and while its input is off nothing in it reacts to keys, pads or the pointer. It starts off. What it
    /// links is given the class <c>rpg-selectable</c>, which the shared stylesheet draws the cursor on while focused.
    /// </summary>
    public sealed class SelectionScope
    {
        internal const string SELECTABLE_CLASS = "rpg-selectable";

        /// <summary>
        /// The player moved the cursor to another element, by a direction or the pointer. Focus the code sets is not a
        /// move.
        /// </summary>
        public event Action Moved;

        private readonly VisualElement m_Root;

        // Held weakly, as a question's answers and the debug room's rows are made and dropped while a scope lives on.
        private readonly ConditionalWeakTable<VisualElement, Selectable> m_Selectables;

        private bool m_InputEnabled;

        // UI Toolkit moves focus by itself — along its own focus ring on a navigation key, to an element the pointer
        // presses — so every event is taken at the root as it trickles down, before it reaches an element.
        public SelectionScope(VisualElement root)
        {
            m_Root         = root;
            m_Selectables  = new ConditionalWeakTable<VisualElement, Selectable>();
            m_InputEnabled = false;

            root.RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationSubmitEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationCancelEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerEnterEvent>(OnPointerEnter, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerDownEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
            root.RegisterCallback<ClickEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);
            root.RegisterCallback<WheelEvent>(StopIfInputDisabled, TrickleDown.TrickleDown);

            root.RegisterCallback<NavigationSubmitEvent>(OnChosen);
            root.RegisterCallback<ClickEvent>(OnChosen);
        }

        /// <summary>Starts or stops the scope reacting to the player.</summary>
        public void SetInputEnabled(bool enabled)
        {
            m_InputEnabled = enabled;
        }

        /// <summary>
        /// Links the elements top to bottom, each one's down the next and up the one before; with
        /// <paramref name="wrap" /> the last leads on to the first and the first back to the last.
        /// </summary>
        public void LinkColumn(VisualElement[] elements, bool wrap)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                Selectable selectable = Add(elements[i]);

                selectable.Up   = Previous(elements, i, wrap);
                selectable.Down = Next(elements, i, wrap);
            }
        }

        /// <summary>
        /// Links the elements left to right, each one's right the next and left the one before; with
        /// <paramref name="wrap" /> the last leads on to the first and the first back to the last.
        /// </summary>
        public void LinkRow(VisualElement[] elements, bool wrap)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                Selectable selectable = Add(elements[i]);

                selectable.Left  = Previous(elements, i, wrap);
                selectable.Right = Next(elements, i, wrap);
            }
        }

        /// <summary>
        /// Where <paramref name="direction" /> takes the cursor from <paramref name="element" />; none leaves the press
        /// to the element, as a slider's row wants left and right.
        /// </summary>
        public void SetNeighbour(VisualElement element, NavigationMoveEvent.Direction direction, VisualElement neighbour)
        {
            Selectable selectable = Add(element);

            switch (direction)
            {
                case NavigationMoveEvent.Direction.Up:
                    selectable.Up = neighbour;
                    break;
                case NavigationMoveEvent.Direction.Down:
                    selectable.Down = neighbour;
                    break;
                case NavigationMoveEvent.Direction.Left:
                    selectable.Left = neighbour;
                    break;
                case NavigationMoveEvent.Direction.Right:
                    selectable.Right = neighbour;
                    break;
            }
        }

        /// <summary>
        /// What choosing <paramref name="element" /> does — a click on it or on anything inside it, or a submit while it
        /// has focus — while the scope's input is on.
        /// </summary>
        public void RegisterChosen(VisualElement element, Action chosen)
        {
            Add(element).Chosen += chosen;
        }

        private Selectable Add(VisualElement element)
        {
            return m_Selectables.GetValue(element, NewSelectable);
        }

        private static Selectable NewSelectable(VisualElement element)
        {
            element.focusable = true;
            element.AddToClassList(SELECTABLE_CLASS);

            return new Selectable();
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

            VisualElement next = FindSelectable(evt.target as VisualElement, out Selectable selectable) == null
                                     ? null
                                     : Neighbour(selectable, evt.direction);

            if (next == null)
            {
                return;
            }

            next.Focus();
            evt.StopImmediatePropagation();

            Moved?.Invoke();
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            if (!m_InputEnabled)
            {
                Stop(evt);
                return;
            }

            VisualElement element = FindSelectable(evt.target as VisualElement, out _);

            if (element == null || element == m_Root.focusController.focusedElement)
            {
                return;
            }

            element.Focus();

            Moved?.Invoke();
        }

        private void OnChosen(EventBase evt)
        {
            if (FindSelectable(evt.target as VisualElement, out Selectable selectable) != null)
            {
                selectable.Chosen?.Invoke();
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

        private VisualElement FindSelectable(VisualElement element, out Selectable selectable)
        {
            selectable = null;

            while (element != null && element != m_Root && !m_Selectables.TryGetValue(element, out selectable))
            {
                element = element.parent;
            }

            return element == m_Root ? null : element;
        }

        private static VisualElement Neighbour(Selectable selectable, NavigationMoveEvent.Direction direction)
        {
            return direction switch
                   {
                       NavigationMoveEvent.Direction.Up    => selectable.Up,
                       NavigationMoveEvent.Direction.Down  => selectable.Down,
                       NavigationMoveEvent.Direction.Left  => selectable.Left,
                       NavigationMoveEvent.Direction.Right => selectable.Right,
                       _                                   => null
                   };
        }

        private static VisualElement Previous(VisualElement[] elements, int index, bool wrap)
        {
            int previous = index > 0 ? index - 1 : wrap ? elements.Length - 1 : -1;

            return previous >= 0 && previous != index ? elements[previous] : null;
        }

        private static VisualElement Next(VisualElement[] elements, int index, bool wrap)
        {
            int next = index < elements.Length - 1 ? index + 1 : wrap ? 0 : -1;

            return next >= 0 && next != index ? elements[next] : null;
        }

        /// <summary>An element the scope links: where each direction takes the cursor from it, and what choosing it does.</summary>
        private sealed class Selectable
        {
            internal VisualElement Up    { get; set; }
            internal VisualElement Down  { get; set; }
            internal VisualElement Left  { get; set; }
            internal VisualElement Right { get; set; }

            internal Action Chosen { get; set; }
        }
    }
}
