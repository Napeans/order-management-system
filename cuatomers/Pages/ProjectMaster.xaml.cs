using cuatomers.DAL;
using napeans.dal;
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

namespace cuatomers.Pages
{
    /// <summary>
    /// Interaction logic for ProjectMaster.xaml
    /// </summary>

    public partial class ProjectMaster : Page
    {
        private readonly IAdoHelper _adoHelper;
        ProcessData _processData;

        public ProjectMaster(IAdoHelper adoHelper)
        {
            InitializeComponent();
            _adoHelper = adoHelper;
            _processData = new ProcessData(_adoHelper);
            LoadCustomerComboBox();  
            startDatePicker.SelectedDate = DateTime.Now;  
        }

        private void LoadCustomerComboBox()
        {
            try
            {
                var customerList = _processData.GetProjectNameByCustomer();

                customerList.Insert(0, new CustomerModel
                {
                    CustomerID = 0,
                    Full_Name = "-- Select Customer --",
                    ProjectName = ""
                });

                customerComboBox.ItemsSource = customerList;
                customerComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load customers: " + ex.Message);
            }
        }


        private void Save_ProjectMaster(object sender, RoutedEventArgs e)
        {
            // Validate inputs before proceeding
            if (!ValidateProjectInputs())
                return;

            string projectName = projectNameTextBox.Text;
            string description = descriptionTextBox.Text;
            DateTime? startDate = startDatePicker.SelectedDate;
            DateTime? endDate = endDatePicker.SelectedDate;
            string contactPerson = contactPersonTextBox.Text;
            string contactNumber = contactNumberTextBox.Text;
            string department = departmentTextBox.Text;

            if (string.IsNullOrWhiteSpace(projectName) || startDate == null)
            {
                MessageBox.Show("Project Name and Start Date are required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var customerMaster = customerComboBox.SelectedItem as CustomerModel;

            if (customerMaster == null || customerMaster.CustomerID == 0)
            {
                MessageBox.Show("Please select a valid customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            bool success = _processData.AddNewProject(
     projectName,
     description,
     startDate.Value,
     endDate,
     contactPerson,
     contactNumber,
     department,
   customerMaster.Full_Name

 );

            if (success)
                MessageBox.Show("Project saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Failed to save project.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            ClearFields();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private bool ValidateProjectInputs()
        {
            // Customer selection
            var selectedCustomer = customerComboBox.SelectedItem as CustomerModel;
            if (selectedCustomer == null || selectedCustomer.CustomerID == 0)
            {
                MessageBox.Show("Please select a valid customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                customerComboBox.Focus();
                return false;
            }


            // Project name
            if (string.IsNullOrWhiteSpace(projectNameTextBox.Text))
            {
                MessageBox.Show("Please enter the project name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                projectNameTextBox.Focus();
                return false;
            }

            // Description
            if (string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                MessageBox.Show("Please enter the description.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                descriptionTextBox.Focus();
                return false;
            }

            // Start date
            if (!startDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select the start date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                startDatePicker.Focus();
                return false;
            }

            // Contact person
            if (string.IsNullOrWhiteSpace(contactPersonTextBox.Text))
            {
                MessageBox.Show("Please enter the contact person.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                contactPersonTextBox.Focus();
                return false;
            }

            // Contact number
            if (string.IsNullOrWhiteSpace(contactNumberTextBox.Text))
            {
                MessageBox.Show("Please enter the contact number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                contactNumberTextBox.Focus();
                return false;
            }

            // Department
            if (string.IsNullOrWhiteSpace(departmentTextBox.Text))
            {
                MessageBox.Show("Please enter the department.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                departmentTextBox.Focus();
                return false;
            }

            // All validations passed
            return true;
        }


        private void ClearFields()
       
        {
            projectNameTextBox.Text = "";
            descriptionTextBox.Text = "";
            startDatePicker.Text = "";
            endDatePicker.Text = "";
            contactPersonTextBox.Text = "";
            contactNumberTextBox.Text = "";
            departmentTextBox.Text = "";
            customerComboBox.SelectedValue = 0;
        }
    }
}
