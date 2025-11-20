using UnityEngine.UIElements;

namespace RPGFramework.Core.Input
{
    public static class UIToolkitInputUtility
    {
        public static void Navigate(NavigationMoveEvent evt,
                                    VisualElement       root,
                                    VisualElement       up    = null,
                                    VisualElement       down  = null,
                                    VisualElement       left  = null,
                                    VisualElement       right = null)
        {
            switch (evt.direction)
            {
                case NavigationMoveEvent.Direction.Up:
                    up?.Focus();
                    break;
                case NavigationMoveEvent.Direction.Down:
                    down?.Focus();
                    break;
                case NavigationMoveEvent.Direction.Left:
                    left?.Focus();
                    break;
                case NavigationMoveEvent.Direction.Right:
                    right?.Focus();
                    break;
            }

            evt.StopPropagation();
            root.focusController.IgnoreEvent(evt);
        }

        public static void RegisterButtonCallbacks(VisualElement                        root,
                                                   EventCallback<NavigationMoveEvent>   navigationEventCallback = null,
                                                   EventCallback<NavigationSubmitEvent> submitEventCallback     = null,
                                                   EventCallback<ClickEvent>            clickEventCallback      = null,
                                                   EventCallback<FocusInEvent>          focusInEventCallback    = null)
        {
            if (navigationEventCallback != null)
            {
                root.RegisterCallback(navigationEventCallback);
            }

            if (submitEventCallback != null)
            {
                root.RegisterCallback(submitEventCallback);
            }

            if (clickEventCallback != null)
            {
                root.RegisterCallback(clickEventCallback);
            }

            if (focusInEventCallback != null)
            {
                root.RegisterCallback(focusInEventCallback);
            }
        }

        public static void UnregisterButtonCallbacks(VisualElement                        root,
                                                     EventCallback<NavigationMoveEvent>   navigationEventCallback = null,
                                                     EventCallback<NavigationSubmitEvent> submitEventCallback     = null,
                                                     EventCallback<ClickEvent>            clickEventCallback      = null,
                                                     EventCallback<FocusInEvent>          focusEventCallback      = null)
        {
            if (navigationEventCallback != null)
            {
                root.UnregisterCallback(navigationEventCallback);
            }

            if (submitEventCallback != null)
            {
                root.UnregisterCallback(submitEventCallback);
            }

            if (clickEventCallback != null)
            {
                root.UnregisterCallback(clickEventCallback);
            }

            if (focusEventCallback != null)
            {
                root.UnregisterCallback(focusEventCallback);
            }
        }
    }
}