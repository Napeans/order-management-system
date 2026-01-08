using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
namespace cuatomers.EventHandlers
{
    public static class BindingHelper
    {
        /// <summary>
        /// Forces all bindings in the visual tree to update their source.
        /// </summary>
        public static void UpdateAllBindings(DependencyObject parent)
        {
            if (parent is FrameworkElement fe)
            {
                foreach (var dp in GetDependencyProperties(fe))
                {
                    var binding = BindingOperations.GetBindingExpression(fe, dp);
                    binding?.UpdateSource();
                }
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                UpdateAllBindings(child);
            }
        }

        /// <summary>
        /// Checks recursively whether the entire form is valid.
        /// </summary>
        public static bool IsFormValid(DependencyObject parent)
        {
            if (Validation.GetHasError(parent))
                return false;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (!IsFormValid(child))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Sets focus to the first control with a validation error.
        /// </summary>
        public static void FocusFirstInvalidControl(DependencyObject parent)
        {
            if (Validation.GetHasError(parent) && parent is UIElement element)
            {
                element.Focus();
                return;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                FocusFirstInvalidControl(child);
            }
        }

        /// <summary>
        /// Returns all dependency properties that are data bound on the element.
        /// </summary>
        private static IEnumerable<DependencyProperty> GetDependencyProperties(FrameworkElement element)
        {
            var properties = new List<DependencyProperty>();

            var localValues = element.GetLocalValueEnumerator();
            while (localValues.MoveNext())
            {
                var entry = localValues.Current;
                if (BindingOperations.IsDataBound(element, entry.Property))
                {
                    properties.Add(entry.Property);
                }
            }

            return properties;
        }
    }
}
