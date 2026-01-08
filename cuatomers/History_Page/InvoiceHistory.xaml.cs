  using cuatomers.DAL;
using cuatomers.Pages;
using cuatomers.Popup_Page;
using napeans.dal;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace cuatomers
{

    public partial class InvoiceHistory : Page
    {
        ProcessData _processData;
        private IAdoHelper _adoHelper;
        public decimal TotalAmount { get; set; }
        public decimal AmountReceived { get; set; }

        public decimal PendingAmount => TotalAmount - AmountReceived;


        public int SelectedInvoiceId { get; set; }



        public InvoiceHistory(IAdoHelper adoHelper)
        {
            InitializeComponent();
            _processData = new ProcessData(adoHelper);
            _adoHelper = adoHelper ?? throw new InvalidOperationException("AdoHelper is null.");
            LoadInvoiceDataGrid();
        }

        private void LoadPayments(int invoiceId)
        {
            var payments = _processData.GetPaymentsByInvoiceId(invoiceId);

            // Compute total paid
            decimal totalPaid = payments.Sum(p => p.PaidAmount);

            // Add a summary row if total is greater than 0
            if (totalPaid > 0)
            {
                payments.Add(new InvoicePaymentModel
                {
                    PaidAmount = totalPaid,
                    PaymentDate = DateTime.MinValue, // empty date or summary label
                    IsSummaryRow = true
                });
            }

            invoiceDataGrid.ItemsSource = payments;
        }

        private void LoadAllPaymentsFromGrid()
        {
            if (invoiceDataGrid.ItemsSource == null) return;

            var invoiceIds = invoiceDataGrid.Items
                .Cast<DataRowView>()
                .Select(row => Convert.ToInt32(row["InvoiceId"]))
                .ToList();

            foreach (int id in invoiceIds)
            {
                LoadPayments(id);
            }
        }





        private void LoadInvoiceDataGrid()
        {
            var ds = _processData.GetInvoiceMasterData(); // This should return a DataSet

            if (ds != null && ds.Tables.Count > 0)
            {
                invoiceDataGrid.ItemsSource = ds.Tables[0].DefaultView;
            }
            //LoadAllPaymentsFromGrid();
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button?.Tag as DataRowView;

            if (row != null)
            {
                int invoiceId = Convert.ToInt32(row["InvoiceId"]);

                var fullInvoice = _processData.GetInvoiceById(invoiceId);
                if (fullInvoice != null)
                {
                    decimal total = fullInvoice.Items.Sum(i => i.Amount);
                    fullInvoice.AmountInWords = GenerateInvoice.ConvertAmountToWords(total.ToString("N2"));

                    var generator = new GenerateInvoice();
                    generator.GenerateInvoicePdf(fullInvoice);
                }
                else
                {
                    MessageBox.Show("Invoice not found.");
                }
            }
            else
            {
                MessageBox.Show("Row data is null or not DataRowView.");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var row = button?.Tag as DataRowView;

            if (row != null)
            {
                string status = row["INVStatus"]?.ToString();
                if (!string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Only invoices with status 'PENDING' can be edited.",
                                    "Edit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int invoiceId = Convert.ToInt32(row["InvoiceID"]);
                var invoiceData = _processData.GetFullInvoiceById(invoiceId);

                if (invoiceData == null)
                {
                    MessageBox.Show("Invoice data not found.");
                    return;
                }

                // Pass data to InvoiceGenerate page (edit mode)
                var editPage = new InvoiceGenerate(_adoHelper, invoiceData);
                NavigationService.Navigate(editPage);
            }
        }



        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var rowView = button?.Tag as DataRowView;

            if (rowView != null)
            {
                int invoiceId = Convert.ToInt32(rowView["InvoiceID"]);
                string invoiceNumber = rowView["INVNumber"].ToString();

                var result = MessageBox.Show(
                    $"Are you sure you want to delete Invoice #{invoiceNumber}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    bool deleted = _processData.SoftDeleteInvoice(invoiceId);
                    if (deleted)
                    {
                        MessageBox.Show("Invoice deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadInvoiceDataGrid();  // Refresh the grid
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Row data is null.");
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var rowView = button?.Tag as DataRowView;

            if (rowView != null)
            {
                int invoiceId = Convert.ToInt32(rowView["InvoiceID"]);

                var fullInvoice = _processData.GetInvoiceById(invoiceId);
                if (fullInvoice != null)
                {
                    decimal total = fullInvoice.Items.Sum(i => i.Amount);
                    fullInvoice.AmountInWords = GenerateInvoice.ConvertAmountToWords(total.ToString("N2"));

                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Title = "Save Invoice PDF",
                        FileName = $"Invoice_{fullInvoice.InvoiceNumber}.pdf",
                        Filter = "PDF Files (*.pdf)|*.pdf"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        var generator = new GenerateInvoice();
                        generator.GenerateInvoicePdfWithDialog(fullInvoice);
                    }
                }
                else
                {
                    MessageBox.Show("Invoice not found.");
                }
            }
            else
            {
                MessageBox.Show("Row data is null.");
            }
        }

        private void btnAddPayment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var rowView = button?.Tag as DataRowView;

            if (rowView != null)
            {
                int invoiceId = Convert.ToInt32(rowView["InvoiceID"]);

                IAdoHelper adoHelper = new AdoHelper();
                var popup = new Payment_Details_Popup(adoHelper)
                {
                    InvoiceId = invoiceId
                };

                if (popup.ShowDialog() == true)
                {
                    LoadInvoiceDataGrid();  
                }
            }
            else
            {
                MessageBox.Show("Row data is null.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnSendtoClient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var rowView = button?.Tag as DataRowView;

            if (rowView != null)
            {
                int invoiceId = Convert.ToInt32(rowView["InvoiceID"]);

                if (rowView.DataView.Table.Columns.Contains("INVStatus"))
                {
                    rowView["INVStatus"] = "In Progress";
                    rowView.Row.AcceptChanges(); // 👈 Forces UI to update color

                    bool success = _processData.UpdateInvoiceStatus(invoiceId, "In Progress");
                    if (!success)
                        MessageBox.Show("Failed to update status in database.");
                }
                else
                {
                    MessageBox.Show("INVStatus column missing in DataTable.");
                }
            }


        }

    }
    public class PendingAmountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is decimal total && values[1] is decimal received)
                return total - received;

            return 0m;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
    public class PendingAmountBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is decimal total && values[1] is decimal received)
            {
                decimal pending = total - received;
                return pending > 0 ? Brushes.Red : Brushes.Green;
            }

            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}


