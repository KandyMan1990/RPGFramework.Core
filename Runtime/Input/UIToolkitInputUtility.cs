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

        public static void RegisterButtonCallbacks(VisualElement                            root,
                                                   EventCallback<NavigationMoveEvent>       navigationEventCallback = null,
                                                   EventCallback<NavigationSubmitEvent>     submitEventCallback     = null,
                                                   EventCallback<ClickEvent>                clickEventCallback      = null,
                                                   EventCallback<FocusEvent, Button>        focusEventCallback      = null,
                                                   EventCallback<BlurEvent, Button>         blurEventCallback       = null,
                                                   EventCallback<PointerEnterEvent, Button> enterEventCallback      = null)
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

            if (focusEventCallback != null)
            {
                root.RegisterCallback(focusEventCallback, (Button)root);
            }

            if (blurEventCallback != null)
            {
                root.RegisterCallback(blurEventCallback, (Button)root);
            }

            if (enterEventCallback != null)
            {
                root.RegisterCallback(enterEventCallback, (Button)root);
            }
        }

        public static void UnregisterButtonCallbacks(VisualElement                            root,
                                                     EventCallback<NavigationMoveEvent>       navigationEventCallback = null,
                                                     EventCallback<NavigationSubmitEvent>     submitEventCallback     = null,
                                                     EventCallback<ClickEvent>                clickEventCallback      = null,
                                                     EventCallback<FocusEvent, Button>        focusEventCallback      = null,
                                                     EventCallback<BlurEvent, Button>         blurEventCallback       = null,
                                                     EventCallback<PointerEnterEvent, Button> enterEventCallback      = null)
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

            if (blurEventCallback != null)
            {
                root.UnregisterCallback(blurEventCallback);
            }

            if (enterEventCallback != null)
            {
                root.UnregisterCallback(enterEventCallback);
            }
        }
    }
}