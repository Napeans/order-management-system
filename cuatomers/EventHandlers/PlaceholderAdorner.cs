using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;

namespace cuatomers.EventHandlers
{
    public class PlaceholderAdorner : Adorner
    {
        private readonly TextBlock placeholderText;

        public PlaceholderAdorner(UIElement adornedElement, string text) : base(adornedElement)
        {
            IsHitTestVisible = false;

            placeholderText = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Gray,
                Margin = new Thickness(5, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement is TextBox textBox)
            {
                var formattedText = new FormattedText(
                    placeholderText.Text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch),
                    textBox.FontSize,
                    placeholderText.Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                // 🔥 Center vertically in the TextBox
                double x = 5; // left padding
                double y = (textBox.ActualHeight - formattedText.Height) / 2;

                drawingContext.DrawText(formattedText, new Point(x, y));
            }
        }
    }
}
