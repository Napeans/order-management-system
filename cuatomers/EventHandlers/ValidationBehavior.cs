using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace cuatomers.EventHandlers
{
    public static class ValidationBehavior
    {
        public static readonly DependencyProperty EnableValidationProperty =
            DependencyProperty.RegisterAttached(
                "EnableValidation",
                typeof(string),
                typeof(ValidationBehavior),
                new PropertyMetadata(null, OnEnableValidationChanged));

        public static void SetEnableValidation(UIElement element, string value) => element.SetValue(EnableValidationProperty, value);
        public static string GetEnableValidation(UIElement element) => (string)element.GetValue(EnableValidationProperty);

        private static void OnEnableValidationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                var binding = new Binding("Text")
                {
                    Source = textBox,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay,
                    ValidatesOnDataErrors = true,
                    NotifyOnValidationError = true
                };

                string[] rules = (e.NewValue as string)?.Split(',') ?? Array.Empty<string>();
                foreach (string rule in rules)
                {
                    switch (rule.Trim().ToLower())
                    {
                        case "required":
                            binding.ValidationRules.Add(new NotEmptyValidationRule());
                            break;
                        case "email":
                            binding.ValidationRules.Add(new EmailValidationRule());
                            break;
                        case "mobile":
                            binding.ValidationRules.Add(new MobileValidationRule());
                            break;
                    }
                }

                textBox.SetBinding(TextBox.TextProperty, binding);
            }
        }
    }
}
