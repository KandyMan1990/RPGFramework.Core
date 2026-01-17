using UnityEngine.UIElements;

namespace RPGFramework.Core.Input
{
    public static class UIToolkitInputUtility
    {
        public static bool Navigate(NavigationMoveEvent evt,
                                    VisualElement       root,
                                    VisualElement       up    = null,
                                    VisualElement       down  = null,
                                    VisualElement       left  = null,
                                    VisualElement       right = null)
        {
            bool navigateSuccess = false;
            
            switch (evt.direction)
            {
                case NavigationMoveEvent.Direction.Up:
                    if (up != null)
                    {
                        up.Focus();
                        navigateSuccess = true;
                    }
                    break;
                case NavigationMoveEvent.Direction.Down:
                    if (down != null)
                    {
                        down.Focus();
                        navigateSuccess = true;
                    }
                    break;
                case NavigationMoveEvent.Direction.Left:
                    if (left != null)
                    {
                        left.Focus();
                        navigateSuccess = true;
                    }
                    break;
                case NavigationMoveEvent.Direction.Right:
                    if (right != null)
                    {
                        right.Focus();
                        navigateSuccess = true;
                    }
                    break;
            }

            evt.StopPropagation();
            root.focusController.IgnoreEvent(evt);

            return navigateSuccess;
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