using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace cuatomers.EventHandlers
{
    public static class FormValidator
    {
        public static bool IsValid(DependencyObject root)
        {
            if (Validation.GetHasError(root))
                return false;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (!IsValid(child))
                    return false;
            }

            return true;
        }

        public static void FocusFirstInvalidControl(DependencyObject root)
        {
            if (Validation.GetHasError(root) && root is UIElement element)
            {
                element.Focus();
                return;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                FocusFirstInvalidControl(child);
            }
        }
    }
}
