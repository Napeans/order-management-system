using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using cuatomers.DAL;

using cuatomers.Pages;
using MaterialDesignThemes.Wpf;
using napeans.dal;
using PdfSharp.Quality;

namespace cuatomers
{
    /// <summary>
    /// Interaction logic for QuoteHistory.xaml
    /// </summary>
    public partial class QuoteHistory : Page
    {
        ProcessData _processData;
        public QuoteHistory(IAdoHelper adoHelper)
        {
            InitializeComponent();
            _processData = new ProcessData(adoHelper);
            LoadQuoteDataGrid();
         

        }
        private void LoadQuoteDataGrid()
        {
            DataSet ds = _processData.GetQuoteMasterData(); // returns DataSet

            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0]; // ✅ Get first DataTable
                quoteDataGrid.ItemsSource = dt.DefaultView;
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button?.DataContext as DataRowView;

            if (row != null)
            {
                int quoteId = Convert.ToInt32(row["QuoteID"]);

                var fullQuote = _processData.GetQuotationById(quoteId);
                if (fullQuote != null)
                {
                    decimal total = fullQuote.Items.Sum(i => i.Amount);
                    fullQuote.AmountInWords = GenerateQuote.ConvertAmountToWords(total.ToString("N2"));

                    var generator = new GenerateQuote();
                    generator.GenerateQuotationPdf(fullQuote); // ✅ internally saves & opens
                }
                else
                {
                    MessageBox.Show("Quotation not found.");
                }
            }
            else
            {
                MessageBox.Show("Row data is null or not DataRowView.");
            }
        }

        private void EditQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button?.DataContext as DataRowView;


            if (row != null)
            {
                //string status = row["QuoteStatus"]?.ToString();
                //if (!string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase))
                //{
                //    MessageBox.Show("Only quotations with status 'PENDING' can be edited.",
                //                    "Edit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                int quoteId = Convert.ToInt32(row["QuoteID"]);
                var quoteData = _processData.GetFullQuotationById(quoteId); // Make sure this method exists

                if (quoteData == null)
                {
                    MessageBox.Show("Quotation data not found.");
                    return;
                }

                // Navigate to the quotation edit page
                IAdoHelper _adoHelper = new AdoHelper();
                var editPage = new QuoteGenerate(_adoHelper); // Ensure this constructor exists
                NavigationService.Navigate(editPage);
            }
        }



        private void LoadAllQuotes()
{
    var quotes = _processData.GetAllQuotations();
    quoteDataGrid.ItemsSource = quotes;
}




        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            DataRowView rowView = button?.DataContext as DataRowView;
            if (rowView != null)
            {
                int quoteId = Convert.ToInt32(rowView["QuoteID"]);

                var result = MessageBox.Show("Are you sure you want to delete this quote?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = _processData.SetQuotationInactive(quoteId);

                    if (success)
                    {
                        rowView.Row.Delete(); // ✅ Remove row from UI
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete quotation from database.", "Error");
                    }
                }
            }
        }



        private void downloadbutton_click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button?.DataContext as DataRowView;

            if (row != null)
            {
                int quoteId = Convert.ToInt32(row["QuoteID"]);

                // Fetch quotation details from DB
                var quoteModel = _processData.GetQuotationById(quoteId);
                if (quoteModel != null)
                {
                    // Ask user where to save the PDF
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = $"Quotation_{quoteModel.QuoteNumber}", // Default file name
                        DefaultExt = ".pdf",                              // Default extension
                        Filter = "PDF documents (.pdf)|*.pdf"             // Filter files by extension
                    };

                    bool? result = dialog.ShowDialog();

                    if (result == true)
                    {
                        string savePath = dialog.FileName;

                        try
                        {
                            var pdfGenerator = new GenerateQuote(); // ✅ Your PDF generation class
                            pdfGenerator.GenerateQuotationPdfWithDialog(quoteModel); // ✅ Must match signature

                            MessageBox.Show("Quotation downloaded to:\n" + savePath,
                                            "Download Successful",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to generate quotation PDF.\n" + ex.Message,
                                            "Error",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Quotation data not found.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        private void AddPoInfo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button?.DataContext as DataRowView;  
            if (row != null)
            {
                int quoteId = Convert.ToInt32(row["QuoteID"]);
                var window = new AddPoInfo(quoteId);  
                window.ShowDialog();
                
            }
        }


    }


}






