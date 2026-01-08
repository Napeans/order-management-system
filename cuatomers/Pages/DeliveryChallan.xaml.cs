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
    /// Interaction logic for DeliveryChallan.xaml
    /// </summary>
    public partial class DeliveryChallan : Page
    {
        ProcessData processData;
        private int serialCounter = 2;
        public DeliveryChallan(IAdoHelper adoHelper)
        {
            InitializeComponent();
            processData = new ProcessData(adoHelper);
            LoadProjectComboBox();
            LoadCustomerComboBox();
            txtDCNumber.Text = processData.GenerateNextDeliveryChallanNumber();
            dpDCDate.SelectedDate = DateTime.Now;
        }

        private void LoadProjectComboBox()
        {
            try
            {
                var projectList = processData.GetProjectName(); // Should return List<ProjectMasterModel>

                projectList.Insert(0, new ProjectMasterModel
                {
                    Project_ID = 0,
                    Project_Name = "-- Select Project --",
                    CustomerName = ""
                });

               
                cmbProject.DisplayMemberPath = "Project_Name";      // ✅ used for display
                cmbProject.SelectedValuePath = "Project_ID";
                cmbProject.ItemsSource = projectList;// ✅ used for value binding
                cmbProject.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load projects: " + ex.Message);
            }
        }

        private void cmbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProject.SelectedItem is ProjectMasterModel selectedProject)
            {
                string customerNameFromProject = selectedProject.CustomerName?.Trim();

                if (!string.IsNullOrWhiteSpace(customerNameFromProject))
                {
                    foreach (var item in cmbSelectedCustomer.Items)
                    {
                        if (item is CustomerModel customer &&
                            (
                                customer.CustomerName?.Trim().Equals(customerNameFromProject, StringComparison.OrdinalIgnoreCase) == true ||
                                customer.Full_Name?.Trim().Equals(customerNameFromProject, StringComparison.OrdinalIgnoreCase) == true
                            ))
                        {
                            cmbSelectedCustomer.SelectedItem = customer;

                            // Trigger visibility updates if needed
                            //cmbSelectedCustomer_SelectionChanged(null, null);
                            return;
                        }
                    }
                }

                // No match found — reset to default
                cmbSelectedCustomer.SelectedIndex = 0;

                // Clear address panels
                txtBillingAddress.Visibility = Visibility.Collapsed;
                txtblkBilling.Visibility = Visibility.Collapsed;
                txtShipping.Visibility = Visibility.Collapsed;
                txtShippingAddress.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadCustomerComboBox()
        {
            var customerList = processData.GetCustomers();

            customerList.Insert(0, new CustomerModel
            {
                CustomerID = 0,
                Full_Name = "-- Select Customer --"
            });

            cmbSelectedCustomer.ItemsSource = customerList;
            cmbSelectedCustomer.DisplayMemberPath = "Full_Name";
            cmbSelectedCustomer.SelectedValuePath = "Full_Name";
            cmbSelectedCustomer.SelectedIndex = 0;
        }

        private void cmbSelectedCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSelectedCustomer.SelectedValue == null)
            {
                txtBillingAddress.Visibility = Visibility.Collapsed;
                txtblkBilling.Visibility = Visibility.Collapsed;

                txtShipping.Visibility = Visibility.Collapsed;
                txtShippingAddress.Visibility = Visibility.Collapsed;
                return;
            }

            string selectedName = cmbSelectedCustomer.SelectedValue.ToString();

            var customerDetails = processData.GetCustomerDetailsByFullName(selectedName);
            if (customerDetails != null)
            {
                txtBillingAddress.Text = customerDetails.BillingAddress;
                txtBillingAddress.Visibility = Visibility.Visible;
                txtblkBilling.Visibility = Visibility.Visible;

            }
            else
            {
                txtBillingAddress.Text = "";
                txtBillingAddress.Visibility = Visibility.Collapsed;
                txtblkBilling.Visibility = Visibility.Visible;
            }

            var customerShippingAddress = processData.GetCustomerShippingAddress(selectedName);
            if (customerShippingAddress != null)
            {
                txtShippingAddress.Text = customerDetails.BillingAddress;
                txtShippingAddress.Visibility = Visibility.Visible;
                txtShipping.Visibility = Visibility.Visible;

            }
            else
            {
                txtShippingAddress.Text = "";
                txtShippingAddress.Visibility = Visibility.Collapsed;
                txtShipping.Visibility = Visibility.Visible;
            }
        }

        private void btnSaveAndSend_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Input validation (optional but recommended)
            if (string.IsNullOrWhiteSpace(txtDCNumber.Text))
            {
                MessageBox.Show("Please enter the Challan Number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpDCDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a valid Challan Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Step 2: Collect form data
            string challanNumber = txtDCNumber.Text.Trim();
            DateTime challanDate = dpDCDate.SelectedDate ?? DateTime.Today;
            string billTo = txtBillingAddress.Text.Trim();
            string shipTo = txtShippingAddress.Text.Trim();
            string remarks = txtPurpose.Text.Trim();

            // Step 3: Save to DeliveryChallanMaster
            int challanId = processData.AddNewDeliveryChallan(
                challanNumber,
                challanDate,
                billTo,
                shipTo,
                remarks
            );

            if (challanId <= 0)
            {
                MessageBox.Show("Failed to save delivery challan.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Step 4: Save items from UI
            bool itemsSaved = ExtractAndSaveAllDCItems(challanId);
            if (!itemsSaved)
            {
                MessageBox.Show("Challan saved, but some items failed to save.", "Partial Failure", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Step 5: Load saved items
            var savedItems = processData.GetDeliveryChallanItemsByDCId(challanId);

            // Step 6: Construct model for PDF
            var challanModel = new GenerateDeliveryChallanModel
            {
                ChallanNumber = challanNumber,
                ChallanDate = challanDate,
                BillToAddress = billTo,
                ShipToAddress = shipTo,
                Remarks = remarks,
                Items = savedItems
                    .Select(i => new ChallanItemModel
                    {
                        Description = i.Description,
                        Quantity = i.Quantity,
                        Uom = i.Uom
                    })
                    .ToList()
            };

            // Step 7: Generate PDF
            var generator = new GenerateDeliveryChallan();
            generator.GeneratePdf(challanModel);

            MessageBox.Show("Delivery Challan saved and PDF generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Optional: Clear form or reload fresh state
            // refresh();
        }

        private void btnAddNewRow_Click(object sender, RoutedEventArgs e)
        {
            Grid itemGrid = new Grid
            {
                Height = 80,
                Background = Brushes.White,
                Margin = new Thickness(0, 5, 0, 0)
            };

            // Columns: S.No, Item, Qty, UOM, Delete
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });   // S.No
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Item Description
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });  // Quantity
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });  // UOM
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });   // Delete

            // Serial Number
            TextBlock txtSerialNo = new TextBlock
            {
                Text = serialCounter.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Medium
            };
            Grid.SetColumn(txtSerialNo, 0);
            itemGrid.Children.Add(txtSerialNo);

            // Item Description
            TextBox txtItemDescription = new TextBox
            {
                Text = "Enter Item Details",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 50,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0),
                Foreground = Brushes.Black,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            Grid.SetColumn(txtItemDescription, 1);
            itemGrid.Children.Add(txtItemDescription);

            // Quantity
            TextBox txtItemQty = new TextBox
            {
                Text = "1",
                Width = 60,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            Grid.SetColumn(txtItemQty, 2);
            itemGrid.Children.Add(txtItemQty);

            // UOM ComboBox
            ComboBox comboUOM = new ComboBox
            {
                Width = 100,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = (Style)FindResource("MaterialDesignComboBox")
            };
            comboUOM.Items.Add(new ComboBoxItem { Content = "Nos" });
            comboUOM.Items.Add(new ComboBoxItem { Content = "Kg" });
            comboUOM.Items.Add(new ComboBoxItem { Content = "Litre" });
            comboUOM.Items.Add(new ComboBoxItem { Content = "Box" });
            comboUOM.SelectedIndex = 0; // Default selection
            Grid.SetColumn(comboUOM, 3);
            itemGrid.Children.Add(comboUOM);

            // Delete Button
            Button btnRemove = new Button
            {
                Content = "✕",
                Style = (Style)FindResource("SecondaryButton"),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 87, 34)),
                FontSize = 14,
                Width = 30,
                Height = 30,
                Padding = new Thickness(0),
                Cursor = Cursors.Hand
            };
            btnRemove.Click += (s, ev) =>
            {
                var result = MessageBox.Show("Are you sure you want to delete this row?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ItemPanel.Children.Remove(itemGrid);
                    // Optional: refresh serial numbers
                }
            };

            StackPanel deletePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            deletePanel.Children.Add(btnRemove);
            Grid.SetColumn(deletePanel, 4);
            itemGrid.Children.Add(deletePanel);

            ItemPanel.Children.Add(itemGrid);
            serialCounter++;
        }

        private bool ExtractAndSaveAllDCItems(int deliveryChallanId)
        {
            var items = new List<DeliveryChallanItemModel>();

            // 1. Static Item (if present)
            string desc = txtStaticItemDescription.Text?.Trim();
            int qty = int.TryParse(txtStaticItemQty.Text, out int parsedQty) ? parsedQty : 0;
            string uom = "";

            // UOM Handling
            if (cmbStaticUOM.SelectedItem is ComboBoxItem selectedUomItem)
                uom = selectedUomItem.Content?.ToString()?.Trim() ?? "";
            else if (cmbStaticUOM.SelectedItem is string selectedUomStr)
                uom = selectedUomStr.Trim();
            else
                uom = cmbStaticUOM.Text?.Trim() ?? "";

            // If only static item is added (no dynamic rows)
            if (ItemPanel.Children.Count == 0 && !string.IsNullOrWhiteSpace(desc))
            {
                items.Add(new DeliveryChallanItemModel
                {
                    DeliveryChallanId = deliveryChallanId,
                    Description = desc,
                    Quantity = qty,
                    Uom = uom
                });
            }

            // 2. Dynamic Items (from UI)
            var challanModel = GetChallanModelFromUI();

            foreach (var item in challanModel.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    items.Add(new DeliveryChallanItemModel
                    {
                        DeliveryChallanId = deliveryChallanId,
                        Description = item.Description,
                        Quantity = item.Quantity,
                        Uom = item.Uom
                    });
                }
            }

            // 3. Remove duplicates (optional)
            items = items
                .GroupBy(x => new { x.Description, x.Quantity, x.Uom })
                .Select(g => g.First())
                .ToList();

            // 4. Save to database
            foreach (var item in items)
            {
                bool saved = processData.AddDeliveryChallanItem(
                    item.DeliveryChallanId,
                    item.Description,
                    item.Quantity,
                    item.Uom
                );

                if (!saved)
                    return false; // Stop on first failure
            }

            return true;
        }

        public GenerateDeliveryChallanModel GetChallanModelFromUI()
        {
            var challanModel = new GenerateDeliveryChallanModel
            {
                ChallanNumber = txtDCNumber.Text.Trim(),
                ChallanDate = dpDCDate.SelectedDate ?? DateTime.Now,
                BillToAddress = txtBillingAddress.Text.Trim(),
                ShipToAddress = txtShippingAddress.Text.Trim(),
                Remarks = txtPurpose.Text.Trim(),
                Items = new List<ChallanItemModel>()
            };

            foreach (Grid itemGrid in ItemPanel.Children)
            {
                string description = "";
                string uom = "";
                int quantity = 0;

                foreach (UIElement element in itemGrid.Children)
                {
                    int col = Grid.GetColumn(element);

                    switch (col)
                    {
                        case 1: // Description
                            if (element is TextBox descBox)
                                description = descBox.Text.Trim();
                            break;

                        case 2: // Quantity
                            if (element is TextBox qtyBox &&
                                int.TryParse(qtyBox.Text.Trim(), out var qty))
                                quantity = qty;
                            break;

                        case 3: // UOM
                            if (element is ComboBox uomCombo)
                            {
                                if (uomCombo.SelectedItem is ComboBoxItem uomItem)
                                    uom = uomItem.Content?.ToString()?.Trim() ?? "";
                                else if (uomCombo.SelectedItem is string str)
                                    uom = str.Trim();
                                else
                                    uom = uomCombo.Text.Trim();
                            }
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    challanModel.Items.Add(new ChallanItemModel
                    {
                        Description = description,
                        Quantity = quantity,
                        Uom = uom
                    });
                }
            }

            return challanModel;
        }

        private void btnDCHistory_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new DC_History(adoHelper));
            }
            else
            {
                MessageBox.Show("MainWindow not found. Are you running in the Designer?");
            }
        }
    }
}
 