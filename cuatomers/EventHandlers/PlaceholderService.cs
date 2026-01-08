using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace cuatomers.EventHandlers
{
    public static class PlaceholderService
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(PlaceholderService),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderProperty);
        }

        public static void SetPlaceholder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded -= TextBox_Loaded;
                textBox.Loaded += TextBox_Loaded;

                textBox.TextChanged -= TextBox_TextChanged;
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            ShowOrHidePlaceholder(textBox);
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            ShowOrHidePlaceholder(textBox);
        }

        private static void ShowOrHidePlaceholder(TextBox textBox)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(textBox);

            if (string.IsNullOrEmpty(textBox.Text))
            {
                var adorners = layer?.GetAdorners(textBox);
                if (adorners == null)
                {
                    layer?.Add(new PlaceholderAdorner(textBox, GetPlaceholder(textBox)));
                }
            }
            else
            {
                var adorners = layer?.GetAdorners(textBox);
                if (adorners != null)
                {
                    foreach (var adorner in adorners)
                    {
                        if (adorner is PlaceholderAdorner)
                        {
                            layer?.Remove(adorner);
                        }
                    }
                }
            }
        }
    }
}
