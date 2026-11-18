using System.Collections;
using RPGFramework.Core.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Tests.UI
{
    /// <summary>
    /// Three labels linked down a wrapping column in a scope, in an editor window's panel, as a menu's are: focus and
    /// UI Toolkit's events need a panel.
    /// </summary>
    internal sealed class ScopePanel
    {
        private const int MOST_FRAMES = 300;

        internal SelectionScope Scope    { get; }
        internal Label[]        Elements { get; }
        internal VisualElement  Root     { get; }

        private readonly EditorWindow m_Window;

        internal ScopePanel()
        {
            m_Window = EditorWindow.GetWindow<PanelWindow>();
            Root     = new VisualElement();
            Elements = new[] { new Label("First"), new Label("Second"), new Label("Third") };

            for (int i = 0; i < Elements.Length; i++)
            {
                Root.Add(Elements[i]);
            }

            m_Window.rootVisualElement.Add(Root);

            Scope = new SelectionScope(Root);
            Scope.LinkColumn(Elements, true);
        }

        internal VisualElement Focused => Root.focusController.focusedElement as VisualElement;

        /// <summary>Repaints until the labels are laid out, as an element takes focus only once its style is known.</summary>
        internal IEnumerator LayOut()
        {
            for (int i = 0; i < MOST_FRAMES && Elements[0].layout.height <= 0f; i++)
            {
                m_Window.Repaint();

                yield return null;
            }
        }

        internal void Move(VisualElement from, NavigationMoveEvent.Direction direction)
        {
            using (NavigationMoveEvent evt = NavigationMoveEvent.GetPooled(direction))
            {
                from.SendEvent(evt);
            }
        }

        internal void Submit(VisualElement on)
        {
            using (NavigationSubmitEvent evt = NavigationSubmitEvent.GetPooled())
            {
                on.SendEvent(evt);
            }
        }

        internal void Close()
        {
            m_Window.Close();
        }

        internal sealed class PanelWindow : EditorWindow
        {
        }
    }
}
