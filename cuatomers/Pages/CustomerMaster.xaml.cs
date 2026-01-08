using cuatomers;
using cuatomers.DAL;
using cuatomers.EventHandlers;
using Microsoft.Win32;
using napeans.dal;
using PdfSharp.Quality;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;


namespace cuatomers.Pages
{


    public partial class CustomerMaster : Page


    {

        private int? editingCustomerId = null;
        ProcessData processData;
        public Customers EditedCustomers;
        private ClickEvent _clickEvent;
        private readonly IAdoHelper _helper;

       

        public CustomerMaster(IAdoHelper adohelper)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            InitializeComponent();
            processData = new ProcessData(adohelper);
            //LoadCustomers();
            this.DataContext = this;



            _clickEvent = new ClickEvent();
            //_clickEvent.SetupPlaceholder(customerfirstname, " Enter your First Name");
            //_clickEvent.SetupPlaceholder(customerlastname, "Enter Your Last Name");
            //_clickEvent.SetupPlaceholder(CustomerContact, " Enter Your Phone Number");
            //_clickEvent.SetupPlaceholder(customerCompanyname, "Enter your Company Name");
          
            //_clickEvent.SetupPlaceholder(CustomarEmail, "Enter Your Email Address");
            //_clickEvent.SetupPlaceholder(txtAttention, "Enter Attention");
            //_clickEvent.SetupPlaceholder(txtStreet1, "Enter   Address 1");
            //_clickEvent.SetupPlaceholder(txtStreet2, "Enter   Address 2");
            //_clickEvent.SetupPlaceholder(txtCity, "Enter  City");
            //_clickEvent.SetupPlaceholder(txtPin, "Enter Pincoad");
            //_clickEvent.SetupPlaceholder(txtphone, "Enter Contect Number");
            //_clickEvent.SetupPlaceholder(txtfax, "Enter Fax Number");
            //_clickEvent.SetupPlaceholder(shipAttention, "Enter Attention");
            //_clickEvent.SetupPlaceholder(shipadd1, "Enter  Address 1");
            //_clickEvent.SetupPlaceholder(shipadd2, "Enter  Address 2");
            //_clickEvent.SetupPlaceholder(shipcity, "Enter City");
            //_clickEvent.SetupPlaceholder(shippin, "Enter Pincoade");
            //_clickEvent.SetupPlaceholder(shipphone, "Enter Condect Number");
            //_clickEvent.SetupPlaceholder(shipfax, "Enter Your Fax Number");
            //_clickEvent.SetupPlaceholder(pannumber, "Enter Pan Card Number");
            //_clickEvent.SetupPlaceholder(openonin, "Enter Opening Balance");
        
        }

        private void NameFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxValidation.ValidateName(customerfirstname, txtfirstNameError);

        }
        private void txtLasttName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxValidation.ValidateName(customerlastname, txtlastNameError);
        }
        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxValidation.ValidateEmail(CustomarEmail, txtError);
        }

        private void txtMobile_TextChanged(object sender, TextChangedEventArgs e)
        {
           
            TextBoxValidation.ValidateMobile(txtphone, txtPhoneError);
            TextBoxValidation.ValidateMobile(shipphone, txtShiPhoneError);
            TextBoxValidation.ValidateMobile(CustomerContact, txtMobileError);
        }

        private void GstNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxValidation.ValidateGST(txtGst, txtGstNumberError);
        }

        private void txtPin_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxValidation.IsValidPincode(shippin, txtShiPinError);
            TextBoxValidation.IsValidPincode(txtPin, txtPinError);
        }


        private void UploadSignature_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Signature Image";
            openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Load image into the UI
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();


                // Optional: Read image bytes to save to DB
                byte[] imageBytes = File.ReadAllBytes(filePath);

                // TODO: Save imageBytes to your database if needed
            }
        }
        private bool IsPageValid(DependencyObject parent)
        {
            // Check if this control has any error
            if (Validation.GetHasError(parent))
                return false;

            // Check all children recursively
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (!IsPageValid(child))
                    return false;
            }

            return true;
        }

        private void Save_Customer(object sender, RoutedEventArgs e)
        {
            if (!IsPageValid(this)) // 'this' is Window/UserControl
            {
                MessageBox.Show("Form is valid. Submitting...");
                return;
                // Save to DB or perform action
            }
            string saulition = (customersaulition.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string firstname = (customerfirstname.Text);
            string lastname = (customerlastname.Text);
            string companyname = (customerCompanyname.Text);
            string gstNumber = (txtGst.Text);
            string email = (CustomarEmail.Text);
            string contact =    (CustomerContact.Text);
            string attention = (txtAttention.Text);
            string country = (comboCountry.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string addressLine1 = (txtStreet1.Text);
            string addressLine2 = (txtStreet2.Text);
            string city = (txtCity.Text);
            string state = (comboState.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string pinCode = (txtPin.Text);
            string phone = (txtphone.Text);
            string fax = (txtfax.Text);
            string shattention = (shipAttention.Text);
            string shcountry = (shipcontry.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string shaddressLine1 =     (shipadd1.Text);
            string shaddressLine2 = (shipadd2.Text);
            string shcity = (shipcity.Text);
            string shstate = (shisate.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string shpinCode = (shippin.Text);
            string shphone = (shipphone.Text);
            string shpfax = (shipfax.Text);
            string OpeningBalance = (openonin.Text);
            string PaymentTerms = (compament.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string PortalLanguage = (portallang.Text);
            string pan = (pannumber.Text);
            string currency = (selectCurrency.Text);

            // 🚫 Validate required fields
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname)) 
            {
                MessageBox.Show("Please enter a valid First Name And Last Name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter a valid Contact email.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(gstNumber))
            {
                MessageBox.Show("Please enter a valid GST Number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(companyname))
            {
                MessageBox.Show("Please enter a valid Company Name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(contact))
            {
                MessageBox.Show("Please enter a valid Contact Number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(attention) || string.IsNullOrEmpty(country) || string.IsNullOrEmpty(addressLine1) || string.IsNullOrEmpty(city)||
               string.IsNullOrEmpty(state) || string.IsNullOrEmpty(pinCode) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(fax))
            {
                MessageBox.Show("Please enter a valid Billing Address.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(shattention) || string.IsNullOrEmpty(shcountry) || string.IsNullOrEmpty(shaddressLine1) || string.IsNullOrEmpty(shcity) ||
             string.IsNullOrEmpty(shstate) || string.IsNullOrEmpty(shpinCode) || string.IsNullOrEmpty(shphone) || string.IsNullOrEmpty(shpfax))
            {
                MessageBox.Show("Please enter a valid Billing Address.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // ✅ Attempt to add customer
            bool success = processData.AddNewCustomer(
                saulition, firstname, lastname, gstNumber, companyname,  email, contact,
                attention, country, addressLine1, addressLine2,
                city, state, pinCode, phone, fax,
                shattention, shcountry, shaddressLine1, shaddressLine2,
                shcity, shstate, shpinCode, shphone, shpfax,
                OpeningBalance, PaymentTerms, PortalLanguage, pan, currency
            );

            if (success)
            {
                MessageBox.Show("Customer saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            else
            {
                MessageBox.Show("Failed to save customer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        

     

            ClearFields();

        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

       

        private void ComboDisplayName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDisplayName();
        }


        private void UpdateDisplayName()
        {
            string salutation = (customersaulition.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string firstName = customerfirstname.Text.Trim();
            string lastName = customerlastname.Text.Trim();

            // Skip placeholder values
            if (salutation == "Salutation") salutation = "";
            if (firstName == "Enter your First Name") firstName = "";
            if (lastName == "Enter Your Last Name") lastName = "";

            string fullName = $"{salutation} {firstName} {lastName}".Trim();

            // Set text only if user actually typed something
        
            }
        




        private void ClearFields()
        {
            customersaulition.SelectedIndex = 0;
            customerfirstname.Text = "";
            customerlastname.Text = "";
            txtGst.Text = "";
            customerCompanyname.Text = "";
            CustomarEmail.Text = "";
            CustomerContact.Text = "";
            txtAttention.Text = "";
            comboCountry.SelectedIndex = 0;
            txtStreet1.Text = "";
            txtStreet2.Text = "";
            txtCity.Text = "";
            comboState.SelectedIndex = 0;
            txtPin.Text = "";
            txtphone.Text = "";
            txtfax.Text = "";

            shipAttention.Text = "";
            shipcontry.SelectedIndex = 0;
            shipadd1.Text = "";
            shipadd2.Text = "";
            shipcity.Text = "";
            shisate.SelectedIndex = 0;
            shippin.Text = "";
            shipphone.Text = "";
            shipfax.Text = "";

            openonin.Text = "";
            pannumber.Text = "";
            portallang.Text = "";
            selectCurrency.SelectedIndex = 0;
            compament.SelectedIndex = 0;
          
        }


        private void SameAsBillingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (shipAttention == null) return;

            shipAttention.Text = txtAttention.Text;
            shipcontry.SelectedValue = comboCountry.SelectedValue;
            shipadd1.Text = txtStreet1.Text;
            shipadd2.Text = txtStreet2.Text;
            shipcity.Text = txtCity.Text;
            shisate.SelectedValue = comboState.SelectedValue;
            shippin.Text = txtPin.Text;
            shipphone.Text = txtphone.Text;
            shipfax.Text = txtfax.Text;
        }




        private void SameAsBillingCheckBox_Unchecked(object sender, RoutedEventArgs e)
{
    if (shipAttention == null) return;

                shipAttention.Clear();
                shipcontry.SelectedItem = comboCountry.Text;
                shipadd1.Clear();
                shipadd2.Clear();
                shipcity.Clear();
                shisate.SelectedItem = comboState.Text;
                shippin.Clear();
                shipphone.Clear();
                shipfax.Clear();
}


    }
}

