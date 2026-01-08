using cuatomers.DAL;
using napeans.dal;
using PdfSharp.Quality;
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

namespace cuatomers.Popup_Page
{
    /// <summary>
    /// Interaction logic for Payment_Details_Popup.xaml
    /// </summary>
    public partial class Payment_Details_Popup : Window
    {
        public DateTime PaymentDate => dpPaymentDate.SelectedDate ?? DateTime.Now;
        public decimal AmountReceived => decimal.TryParse(txtAmountReceived.Text, out var v1) ? v1 : 0;
        public decimal TaxDeducted => decimal.TryParse(txtTaxDeducted.Text, out var v2) ? v2 : 0;
        public decimal IGST => decimal.TryParse(txtIGST.Text, out var v3) ? v3 : 0;
        public decimal CGST => decimal.TryParse(txtCGST.Text, out var v4) ? v4 : 0;
        public int InvoiceId { get; set; } // ✅ This must exist

        ProcessData _processData;
        public Payment_Details_Popup(IAdoHelper adoHelper)
        {
            InitializeComponent();
            _processData = new ProcessData(adoHelper); // Assuming AdoHelper is your data access layer
            dpPaymentDate.SelectedDate = DateTime.Today;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtAmountReceived.Text, out decimal amountReceived) || amountReceived <= 0)
            {
                MessageBox.Show("Please enter a valid Amount Received.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime paymentDate = dpPaymentDate.SelectedDate ?? DateTime.Now;

            decimal.TryParse(txtTaxDeducted.Text, out decimal taxDeducted);
            decimal.TryParse(txtIGST.Text, out decimal igst);
            decimal.TryParse(txtCGST.Text, out decimal cgst);

            int result = _processData.AddInvoicePayment(
                invoiceId: this.InvoiceId, // Assume InvoiceId is passed or bound to this window
                paymentDate: paymentDate,
                amountReceived: amountReceived,
                taxDeducted: taxDeducted,
                igst: igst,
                cgst: cgst
            );

            if (result > 0)
            {
                MessageBox.Show("Payment added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Failed to add payment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
