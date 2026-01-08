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
using System.Windows.Shapes;

namespace cuatomers.Pages
{
    /// <summary>
    /// Interaction logic for PromptWindow.xaml
    /// </summary>
    public partial class PromptWindow : Window
    {
        public DateTime SelectedDate => DateInput.SelectedDate ?? DateTime.Today;
        public string ProjectName => ProjectNameInput.Text;
        public string ExpenseDetails => ExpenseDetailsInput.Text;

        public PromptWindow()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }

}
