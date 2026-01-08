using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

namespace cuatomers.Pages
{
    /// <summary>
    /// Interaction logic for AddPoInfo.xaml
    /// </summary>
    public partial class AddPoInfo : Window
    {
        private string selectedFilePath = "";
        private int quoteId; // Optional: pass selected quote ID
        public AddPoInfo(int quoteId)
        {
            InitializeComponent(); 
            this.quoteId = quoteId;
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                selectedFilePath = dlg.FileName;
                SelectedFileText.Text = System.IO.Path.GetFileName(selectedFilePath); // FIXED
            }
        }

        private void SavePO_Click(object sender, RoutedEventArgs e)
        {
            string poNumber = PoNumberBox.Text;
            DateTime? poDate = PoDatePicker.SelectedDate;
            string remarks = RemarksBox.Text;

            if (string.IsNullOrWhiteSpace(poNumber) || poDate == null || string.IsNullOrWhiteSpace(selectedFilePath))
            {
                MessageBox.Show("Please fill all fields and select a PO file.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string saveFolder = @"C:\YourApp\POFiles";
            Directory.CreateDirectory(saveFolder); // Ensure the folder exists

            string newFileName = System.IO.Path.Combine(saveFolder, System.IO.Path.GetFileName(selectedFilePath)); // FIXED
            File.Copy(selectedFilePath, newFileName, true);

            // TODO: Save to database or perform any additional logic here

            MessageBox.Show("PO Details Saved Successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

