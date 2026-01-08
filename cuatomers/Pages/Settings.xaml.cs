using cuatomers.DAL;
using Microsoft.Win32;
using napeans.dal;
using PdfSharp.Quality;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace cuatomers.Pages
{
    /// <summary>
    /// Interaction logic for InvoiceSettings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        ProcessData processData;
        private string logoPath = "";
        private string signaturePath = "";

        public Settings(IAdoHelper adoHelper)
        {

            InitializeComponent();
            processData = new ProcessData(adoHelper);
            LoadSettingsToUI();
        }

        //private void UploadButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // Open file dialog
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Title = "Select Signature Image";
        //    openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        string filePath = openFileDialog.FileName;

        //        // Load image into the UI
        //        BitmapImage bitmap = new BitmapImage();
        //        bitmap.BeginInit();
        //        bitmap.UriSource = new Uri(filePath);
        //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmap.EndInit();
        //        Photoviw.Source = bitmap;

        //        // Optional: Read image bytes to save to DB
        //        byte[] imageBytes = File.ReadAllBytes(filePath);

        //        // TODO: Save imageBytes to your database if needed
        //    }
        //}
        //private void UploadSignature_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Title = "Select Signature Image";
        //    openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        string filePath = openFileDialog.FileName;

        //        // Load image into the UI
        //        BitmapImage bitmap = new BitmapImage();
        //        bitmap.BeginInit();
        //        bitmap.UriSource = new Uri(filePath);
        //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmap.EndInit();
        //        SignaturePreview.Source = bitmap;

        //        // Optional: Read image bytes to save to DB
        //        byte[] imageBytes = File.ReadAllBytes(filePath);

        //        // TODO: Save imageBytes to your database if needed
        //    }
        //}

        private void UploadLogo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";

            if (dlg.ShowDialog() == true)
            {
                logoPath = dlg.FileName;
                LogoPreview.Source = new BitmapImage(new Uri(logoPath));
            }
        }


        private void UploadSignature_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";

            if (dlg.ShowDialog() == true)
            {
                signaturePath = dlg.FileName;
                SignaturePreview.Source = new BitmapImage(new Uri(signaturePath));
            }
        }


        private void LoadSettingsToUI()
        {
            var settings = processData.LoadAppSettings(); // yourService is the class that has ADOHelper injected

            if (settings != null)
            {
                txtCompanyName.Text = settings.CompanyName;
                txtCompanyAddress.Text = settings.CompanyAddress;
                txtGST.Text = settings.GSTNumber;
                txtEmail.Text = settings.CompanyEmail;
                txtPhone.Text = settings.CompanyPhone;

                cmbPaymentsDue.SelectedItem = $"{settings.PaymentsDueDays} days";
                txtSalesPerson.Text = settings.SalesPersonName;

                txtBankName.Text = settings.BankName;
                txtAccountNumber.Text = settings.AccountNumber;
                txtIFSC.Text = settings.IFSCCode;
                txtBranchAddress.Text = settings.BranchAddress;

                LogoPreview.Source = new BitmapImage(new Uri(settings.CompanyLogoPath));
                SignaturePreview.Source = new BitmapImage(new Uri(settings.AuthorizedSignPath));
            }
        }

        private void SaveSettingsFromUI(object sender, RoutedEventArgs e)
        {
            var settings = new AppSettingsModel
            {
                CompanyName = txtCompanyName.Text,
                CompanyAddress = txtCompanyAddress.Text,
                GSTNumber = txtGST.Text,
                CompanyEmail = txtEmail.Text,
                CompanyPhone = txtPhone.Text,

                PaymentsDueDays = int.Parse((cmbPaymentsDue.SelectedItem as ComboBoxItem)?.Content.ToString().Split(' ')[0] ?? "30"),
                SalesPersonName = txtSalesPerson.Text,

                BankName = txtBankName.Text,
                AccountNumber = txtAccountNumber.Text,
                IFSCCode = txtIFSC.Text,
                BranchAddress = txtBranchAddress.Text,

                CompanyLogoPath = logoPath,               // set by UploadLogo_Click
                AuthorizedSignPath = signaturePath        // set by UploadSignature_Click
            };

            bool success = processData.SaveAppSettings(settings);
            MessageBox.Show(success ? "Settings saved." : "Failed to save settings.");
        }

    }
}
