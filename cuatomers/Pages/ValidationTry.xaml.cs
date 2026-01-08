using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using cuatomers.EventHandlers;

namespace cuatomers.Pages
{
    /// <summary>
    /// Interaction logic for ValidationTry.xaml
    /// </summary>
    public partial class ValidationTry : Page
    {
        public ValidationTry()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            BindingHelper.UpdateAllBindings(this);
            if (FormValidator.IsValid(this))
            {
                MessageBox.Show("Form submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please fix the highlighted errors.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                FormValidator.FocusFirstInvalidControl(this);
            }
        }

    }
}
