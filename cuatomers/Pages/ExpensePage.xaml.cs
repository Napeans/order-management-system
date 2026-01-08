using cuatomers;
using cuatomers.DAL;
using cuatomers.EventHandlers;
using napeans.dal;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;


namespace cuatomers.Pages
{
    public partial class ExpensePage : Page
    {
        private ProcessData processData;
        private IAdoHelper _adoHelper;

        public ExpensePage(IAdoHelper adoHelper)
        {
            InitializeComponent();
            _adoHelper = adoHelper;
            processData = new ProcessData(adoHelper);
            LoadProjectComboBox();
            txtdate.SelectedDate = DateTime.Today;
        }

        private void LoadProjectComboBox()
        {
            try
            {
                var projectList = processData.GetProjectName();

                projectList.Insert(0, new ProjectMasterModel
                {
                    Project_ID = -1,
                    Project_Name = "Other"
                });
                projectList.Insert(0, new ProjectMasterModel
                {
                    Project_ID = 0,
                    Project_Name = "-- Select Project --"
                });

                selectProject.ItemsSource = projectList;
                selectProject.DisplayMemberPath = "Project_Name";
                selectProject.SelectedValuePath = "Project_ID";
                selectProject.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Project Name: " + ex.Message);
            }
        }

        private void selectProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectProject.SelectedItem != null)
            {
                // Cast to your model or use dynamic
                var selected = selectProject.SelectedItem as dynamic;

                // If you're using a ProjectModel, use (ProjectModel) instead of dynamic
                if (selected.Project_Name == "Other")
                {
                    extext.Visibility = Visibility.Visible;
                    OtherProjectTextBox.Visibility = Visibility.Visible;
                }
                else
                {
                    extext.Visibility = Visibility.Collapsed;
                    OtherProjectTextBox.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void txtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxValidation.IsValidvalue(txtexpensevalue, txtValueError);
        }


        private void save_Expense(object sender, RoutedEventArgs e)
        {

            string projectName = selectProject.Text?.Trim();
            string otherExpense = OtherProjectTextBox.Text?.Trim();
            string description = txtexdecriptor.Text?.Trim();
            string expenseValue = txtexpensevalue?.Text?.Trim();
            string spentby = txtspentby?.Text?.Trim();
            DateTime? Exdate = txtdate?.SelectedDate;

            string modeofPay = "";
            if (radioCash.IsChecked == true) modeofPay = "Cash";
            else if (radioUPI.IsChecked == true) modeofPay = "GPay/UPI";
            else if (radioCard.IsChecked == true) modeofPay = "Card";

            if (string.IsNullOrWhiteSpace(projectName))
            {
                MessageBox.Show("Project Name required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(expenseValue) )
            {
                MessageBox.Show("Expense Value required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(spentby) || Exdate == null)
            {
                MessageBox.Show("Spend By And Date required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            bool success = processData.AddExpenseProject(
                projectName,
                otherExpense,
                description,
                modeofPay,
                expenseValue,
                spentby,
                (DateTime)Exdate
            );

            if (success)
            {
                MessageBox.Show("Project saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to save project.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            // Reload the project ComboBox
            LoadProjectComboBox();

            // Reset all form fields
            selectProject.SelectedIndex = 0;
            OtherProjectTextBox.Text = string.Empty;
            OtherProjectTextBox.Visibility = Visibility.Collapsed;

            txtexdecriptor.Text = string.Empty;
            txtexpensevalue.Text = string.Empty;
            txtspentby.Text = string.Empty;
            txtdate.SelectedDate = null;

            radioCash.IsChecked = false;
            radioUPI.IsChecked = false;
            radioCard.IsChecked = false;

            MessageBox.Show("Form refreshed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Viwe_Click(object sender, RoutedEventArgs e)
        {
            var historyPage = new ExpenceHistry(_adoHelper);
            NavigationService?.Navigate(historyPage);
        }

    }

}

