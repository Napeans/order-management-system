using cuatomers.DAL;
using MaterialDesignThemes.Wpf;
using napeans.dal;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PdfSharp.Fonts;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using System.Xml.Linq;
using WpfAnimatedGif;
using System.Globalization;

namespace cuatomers.Pages
{
    /// <summary>
    /// Interaction logic for InvoiceGenerator.xaml
    /// </summary>
    public partial class QuoteGenerate : Page
    {
        ProcessData processData;
        private List<CustomerModel> customerList;
        List<QuotationItemModel> quoteItems = new List<QuotationItemModel>();
        string quoteNo = GenerateQuote.GenerateQuoteNumber();
        private ClickEvent _clickEvent;

        public QuoteGenerate(IAdoHelper adoHelper)
        {
            InitializeComponent();
            processData = new ProcessData(adoHelper);
            OnLoad();
            LoadCustomerComboBox();
            LoadProjectComboBox();

        }

        public void OnLoad()
        {
            
            txtStaticItemQty.TextChanged += StaticRow_TextChanged;
            txtStaticItemRate.TextChanged += StaticRow_TextChanged;
            cmbStaticItemTax.SelectionChanged += StaticRow_TextChanged;
            SetupPlaceholder(txtStaticItemDescription, "Enter A Item.");
            SetupPlaceholder(txtStaticItemQty, "0");
            SetupPlaceholder(txtStaticItemRate, "0");
            SetupPlaceholder(txtSubject, "Let Your Customer Know What This Quote Is For");
            SetupPlaceholder(txtStaticHSN, "HSN");
            txtQuoteNumber.Text = processData.GenerateNextQuoteNumber();

            txtQuoteDate.SelectedDate = DateTime.Now;
            string newQuoteNo = processData.GenerateNextQuoteNumber();
            txtQuoteNumber.Text = newQuoteNo; 
            SetExpiryDateFromQuoteDate();
            txtDiscountPercent.TextChanged += (s, e) => CalculateGrandTotal();
        }

        private void LoadCustomerComboBox()
        {
            var customerList = processData.GetCustomers();

            customerList.Insert(0, new CustomerModel
            {
                CustomerID = 0,
                Full_Name = "-- Select Customer --"
            });

            cmbSelectCustomer.ItemsSource = customerList;
            cmbSelectCustomer.DisplayMemberPath = "Full_Name";
            cmbSelectCustomer.SelectedValuePath = "Full_Name";
            cmbSelectCustomer.SelectedIndex = 0;
        }

        private void cmbSelectCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSelectCustomer.SelectedValue == null)
            {
                txtBillingAddress.Visibility = Visibility.Collapsed;
                txtblkBilling.Visibility = Visibility.Collapsed;

                txtShipping.Visibility = Visibility.Collapsed;
                txtShippingAddress.Visibility = Visibility.Collapsed;
                return;
            }

            string selectedName = cmbSelectCustomer.SelectedValue.ToString();

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

        private void cmbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProject.SelectedItem is ProjectMasterModel selectedProject)
            {
                string customerNameFromProject = selectedProject.CustomerName?.Trim();

                if (!string.IsNullOrWhiteSpace(customerNameFromProject))
                {
                    foreach (var item in cmbSelectCustomer.Items)
                    {
                        if (item is CustomerModel customer &&
                            customer.Full_Name?.Trim().Equals(customerNameFromProject, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            cmbSelectCustomer.SelectedItem = customer;
                            return;
                        }
                    }
                }

                // If no match found
                cmbSelectCustomer.SelectedIndex = 0;
            }
        }

        private void LoadProjectComboBox()
        {
            try
            {
                var projectList = processData.GetProjectName(); // Returns List<ProjectMasterModel>

                projectList.Insert(0, new ProjectMasterModel
                {
                    Project_ID = 0,
                    Project_Name = "-- Select Project --",
                    CustomerName = ""
                });

                cmbProject.ItemsSource = projectList;
                cmbProject.DisplayMemberPath = "Project_Name";
                cmbProject.SelectedValuePath = "Project_Name";
                cmbProject.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load projects: " + ex.Message);
            }
        }

        private void btnAddNewRow_Click(object sender, RoutedEventArgs e)
        {
            // Create the row container
            Grid itemGrid = new Grid
            {
                Height = 60,
                Background = Brushes.White,
                Margin = new Thickness(0, 5, 0, 0)
            };

            // Define columns
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // Item
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // HSN
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // UOM
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Qty
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Rate
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Tax
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Amount
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });  // Delete

            // Item Description
            TextBox txtItemDescription = new TextBox
            {
                Text = "Enter a item.",
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                Margin = new Thickness(20, 0, 0, 0),
                Style = (Style)FindResource("BorderlessTextBox")
            };
            Grid.SetColumn(txtItemDescription, 0);
            itemGrid.Children.Add(txtItemDescription);
            SetupPlaceholder(txtItemDescription, "Enter A Item.");


            // HSN TextBox
            TextBox txtHSN = new TextBox
            {
                Text = "", // Leave empty for placeholder logic
                Width = 90,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)this.FindResource("BorderlessTextBox") // Add `this.` if inside a Window or UserControl
            };
            Grid.SetColumn(txtHSN, 1); // Make sure your Grid has enough columns
            itemGrid.Children.Add(txtHSN);

            // Optional: Setup placeholder if you’ve implemented it
            SetupPlaceholder(txtHSN, "HSN");

            // UOM ComboBox
            ComboBox cmbUOM = new ComboBox
            {
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = (Style)this.FindResource("BorderlessComboBox")
            };

            // Add unit items
            cmbUOM.Items.Add("PCS");
            cmbUOM.Items.Add("KG");
            cmbUOM.Items.Add("Litre");
            cmbUOM.Items.Add("Meter");
            cmbUOM.Items.Add("Box");

            cmbUOM.SelectedIndex = 0; // Optional: default selected
            Grid.SetColumn(cmbUOM, 2);
            itemGrid.Children.Add(cmbUOM);


            // Quantity
            TextBox txtItemQty = new TextBox
            {
                Text = "0",
                Width = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            Grid.SetColumn(txtItemQty, 3);
            SetupPlaceholder(txtItemQty, "0");
            itemGrid.Children.Add(txtItemQty);

            // Rate
            TextBox txtItemRate = new TextBox
            {
                Text = "0",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            Grid.SetColumn(txtItemRate, 4);
            itemGrid.Children.Add(txtItemRate);
            SetupPlaceholder(txtItemRate, "0");

            // Tax ComboBox
            ComboBox cmbItemTax = new ComboBox
            {
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = (Style)FindResource("BorderlessComboBox")
            };
            cmbItemTax.Items.Add("0%");
            cmbItemTax.Items.Add("5%");
            cmbItemTax.Items.Add("12%");
            cmbItemTax.Items.Add("18%");
            cmbItemTax.Items.Add("28%");
            cmbItemTax.SelectedIndex = 0;
            cmbItemTax.Text = "Select Tax";
            Grid.SetColumn(cmbItemTax, 5);
            itemGrid.Children.Add(cmbItemTax);

            // Amount
            TextBlock txtItemAmount = new TextBlock
            {
                Text = "0",
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(txtItemAmount, 6);
            itemGrid.Children.Add(txtItemAmount);

            // Calculate Amount on Qty/Rate/Tax change
            txtItemQty.TextChanged += (s, ev) =>
            {
                UpdateRowAmount(txtItemQty, txtItemRate, cmbItemTax, txtItemAmount);
                CalculateGrandTotal();
            };

            txtItemRate.TextChanged += (s, ev) =>
            {
                UpdateRowAmount(txtItemQty, txtItemRate, cmbItemTax, txtItemAmount);
                CalculateGrandTotal();
            };

            cmbItemTax.SelectionChanged += (s, ev) =>
            {
                UpdateRowAmount(txtItemQty, txtItemRate, cmbItemTax, txtItemAmount);
                CalculateGrandTotal();
            };

            // Remove Button
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button btnRemove = new Button
            {
                Content = "✕",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 87, 34)),
                FontSize = 14
            };
            btnRemove.Click += (s, ev) =>
            {
                var result = MessageBox.Show("Are you sure you want to delete this row?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ItemPanel.Children.Remove(itemGrid);
                    CalculateGrandTotal();
                }
            };

            buttonPanel.Children.Add(btnRemove);
            Grid.SetColumn(buttonPanel, 7);
            itemGrid.Children.Add(buttonPanel);

            // Add final row to panel
            ItemPanel.Children.Add(itemGrid);
        }

        private void StaticRow_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtStaticItemQty.Text, out decimal qty) &&
                decimal.TryParse(txtStaticItemRate.Text, out decimal rate))
            {
                decimal taxRate = 0;

               
                if (cmbStaticItemTax.SelectedItem is ComboBoxItem item)
                {
                    string taxText = item.Content.ToString().TrimEnd('%');
                    decimal.TryParse(taxText, out taxRate);
                }

                decimal subtotal = qty * rate;
                decimal taxAmount = subtotal * (taxRate / 100);
                decimal total = subtotal + taxAmount;

                txtStaticItemAmount.Text = total.ToString("0.00");
            }
            else
            {
                txtStaticItemAmount.Text = "0.00";
            }

            CalculateGrandTotal();
        }
        
        private void CalculateGrandTotal()
        {
            decimal grandTotal = 0;

            // 1. Add amounts from dynamic rows
            foreach (UIElement row in ItemPanel.Children)
            {
                if (row is Grid grid)
                {
                    foreach (UIElement element in grid.Children)
                    {
                        if (element is TextBlock tb && Grid.GetColumn(tb) == 6) // ✅ Column 6 = Amount
                        {
                            if (decimal.TryParse(tb.Text, out decimal amt))
                                grandTotal += amt;
                        }
                    }
                }
            }

            // 2. Apply discount
            if (decimal.TryParse(txtDiscountPercent.Text, out decimal discountPercent) && discountPercent > 0)
            {
                decimal discountAmount = grandTotal * (discountPercent / 100);
                txtDiscountAmount.Text = $"Discount (₹): {discountAmount:0.00}";
                grandTotal -= discountAmount;
            }
            else
            {
                txtDiscountAmount.Text = "Discount (₹): 0.00";
            }

            // 3. Display final total
            txtGrandTotal.Tag = grandTotal; // Store raw value
            txtGrandTotal.Text = $"Total (₹): ₹{grandTotal:0.00}";

        }

        private void UpdateRowAmount(TextBox txtQty, TextBox txtRate, ComboBox cmbTax, TextBlock txtAmount)
        {
            decimal qty = 0, rate = 0, taxRate = 0;

            decimal.TryParse(txtQty.Text, out qty);
            decimal.TryParse(txtRate.Text, out rate);

            if (cmbTax.SelectedItem is string selectedTax)
            {
                decimal.TryParse(selectedTax.TrimEnd('%'), out taxRate);
            }

            decimal subtotal = qty * rate;
            decimal taxAmount = subtotal * (taxRate / 100);
            decimal total = subtotal + taxAmount;

            txtAmount.Text = total.ToString("0.00");

            // Optionally, call a method to recalculate the grand total
            CalculateGrandTotal();
        }

        private void btnSaveAndSend_Click(object sender, RoutedEventArgs e)
        {

            if (!ValidateQuoteFormInputs())
                return;

            var selectedCustomer = (CustomerModel)cmbSelectCustomer.SelectedItem;
            if (selectedCustomer == null)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int customerId = selectedCustomer.CustomerID;
            string quoteNumber = txtQuoteNumber.Text;

            // Parse dates
            DateTime? quoteDate = null;
            DateTime parsedQuoteDate;

            if (DateTime.TryParseExact(
                    txtQuoteDate.Text,
                    "dd-MMM-yyyy",                       // ← match your DatePicker format
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsedQuoteDate))
            {
                quoteDate = parsedQuoteDate;
            }
            else
            {
                MessageBox.Show("Invalid quote date format. Please use dd-MMM-yyyy.", "Date Format Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            DateTime? expiryDate = null;
            DateTime parsedExpiryDate;
            if (DateTime.TryParse(txtExpiryDate.Text, out parsedExpiryDate))
            {
                expiryDate = parsedExpiryDate;
            }

            // Other inputs
            string projectName = cmbProject.Text;
            string subject = txtSubject.Text;
            decimal discountPercent = decimal.TryParse(txtDiscountPercent.Text, out var dp) ? dp : 0;
            string taxType = cmbStaticItemTax.SelectedItem?.ToString() ?? "";
            string input = txtGrandTotal.Text?.Trim().Replace("₹", "").Replace(",", "");

            decimal grandTotal = (decimal)txtGrandTotal.Tag;

            string customerNotes = txtCustomerNotes.Text;
            string termsAndConditions = txtTermsAndConditions.Text;

            // === Save quotation master ===
            int quoteId = processData.AddNewQuotation(
                customerId,
                quoteNumber,
                quoteDate ?? DateTime.Today,
                expiryDate ?? DateTime.Today,
                projectName,
                subject,
                discountPercent,
                taxType,
                grandTotal,
                customerNotes,
                termsAndConditions
            );

            if (quoteId <= 0)
            {
                MessageBox.Show("Failed to save quotation.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

           
            bool itemsSaved = ExtractAndSaveAllQuotationItems(quoteId);

            if (!itemsSaved)
            {
                MessageBox.Show("Quotation saved, but some items failed to save.", "Partial Failure", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            var savedItems = processData.GetQuotationItemsByQuoteId(quoteId);

             decimal savedTotal = savedItems.Sum(i => i.Amount);

             var quote = new GenerateQuotationModel
            {
                QuoteNumber = quoteNumber,
                QuoteDate = quoteDate ?? DateTime.Today,
                ValidityDate = expiryDate ?? DateTime.Today,
                BillToAddress = txtBillingAddress.Text,
                ShipToAddress = txtShippingAddress.Text,
                VendorCode = "THHE001",
                Description = subject,
                Items = savedItems,
                //TotalAmount = savedTotal,
                AmountInWords = GenerateQuote.ConvertAmountToWords(savedTotal.ToString("N2")),
                Notes = customerNotes,
                PaymentTerms = termsAndConditions
            };

            //var generate = new GenerateQuote();
            //generate.GenerateQuotation(quote);

            MessageBox.Show("Quotation and items saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            refresh();
        } 

        private bool ExtractAndSaveAllQuotationItems(int quotationId)
        {
            var items = new List<QuotationItemModel>();

            // 1. STATIC ITEM
            string desc = txtStaticItemDescription.Text?.Trim();
            int qty = int.TryParse(txtStaticItemQty.Text, out int q1) ? q1 : 0;
            decimal rate = decimal.TryParse(txtStaticItemRate.Text, out decimal r1) ? r1 : 0;
            decimal hsn = decimal.TryParse(txtStaticHSN.Text, out decimal h1) ? h1 : 0;
            decimal tax = 0;
            string uom = "";

            if (cmbUOM.SelectedItem is string selectedUomStr)
                uom = selectedUomStr.Trim();
            else if (cmbUOM.SelectedItem is ComboBoxItem selectedUomItem)
                uom = selectedUomItem.Content?.ToString()?.Trim() ?? "";
            else
                uom = cmbUOM.Text?.Trim() ?? "";

            if (cmbStaticItemTax.SelectedItem is string staticTaxStr)
                decimal.TryParse(staticTaxStr.Replace("%", "").Trim(), out tax);
            else if (cmbStaticItemTax.SelectedItem is ComboBoxItem staticTaxItem)
                decimal.TryParse(staticTaxItem.Content?.ToString()?.Replace("%", "").Trim(), out tax);
            else
                decimal.TryParse(cmbStaticItemTax.Text?.Replace("%", "").Trim(), out tax);

            if (ItemPanel.Children.Count == 0 && !string.IsNullOrWhiteSpace(desc))
            {
                items.Add(new QuotationItemModel
                {
                    QuotationId = quotationId,
                    Description = desc,
                    Quantity = qty,
                    Rate = rate,
                    Tax = tax,
                    Hsn = Convert.ToInt32(hsn),
                    Uom = uom
                });
            }

            // 2. DYNAMIC ITEMS (reusing your method)
            var dynamicItems = GetQuotationItemsFromUI();
            foreach (var dynItem in dynamicItems)
            {
                dynItem.QuotationId = quotationId;
                items.Add(dynItem);
            }

            // 3. REMOVE DUPLICATES (optional)
            items = items
                .GroupBy(x => new { x.Description, x.Quantity, x.Rate, x.Tax, x.Hsn, x.Uom })
                .Select(g => g.First())
                .ToList();

            // 4. SAVE TO DATABASE
            foreach (var item in items)
            {
                bool saved = processData.AddQuotationItem(
                    item.QuotationId,
                    item.Description,
                    item.Quantity,
                    item.Rate,
                    item.Tax,
                    item.Hsn,
                    item.Uom
                );

                if (!saved)
                    return false; // Stop on first failure
            }

            return true;
        } 

        public List<QuotationItemModel> GetQuotationItemsFromUI()
        {
            var items = new List<QuotationItemModel>();

            foreach (Grid itemGrid in ItemPanel.Children)
            {
                if (itemGrid is Grid grid)
                {
                    string description = "";
                    int quantity = 0;
                    decimal rate = 0;
                    decimal tax = 0;
                    int hsn = 0;
                    string uom = "";

                    foreach (UIElement element in grid.Children)
                    {
                        int col = Grid.GetColumn(element);

                        switch (col)
                        {
                            case 0: // Description
                                if (element is TextBox descBox)
                                    description = descBox.Text?.Trim();
                                break;

                            case 1: // HSN
                                if (element is TextBox hsnBox &&
                                    int.TryParse(hsnBox.Text?.Trim(), out var parsedHsn))
                                    hsn = parsedHsn;
                                break;

                            case 2: // UOM
                                if (element is ComboBox uomCombo)
                                {
                                    if (uomCombo.SelectedItem is string strUom)
                                        uom = strUom.Trim();
                                    else if (uomCombo.SelectedItem is ComboBoxItem cbUom)
                                        uom = cbUom.Content?.ToString()?.Trim() ?? "";
                                    else
                                        uom = uomCombo.Text?.Trim() ?? "";
                                }
                                break;

                            case 3: // Quantity
                                if (element is TextBox qtyBox &&
                                    int.TryParse(qtyBox.Text?.Trim(), out var parsedQty))
                                    quantity = parsedQty;
                                break;

                            case 4: // Rate
                                if (element is TextBox rateBox &&
                                    decimal.TryParse(rateBox.Text?.Trim(), out var parsedRate))
                                    rate = parsedRate;
                                break;

                            case 5: // Tax
                                if (element is ComboBox taxCombo)
                                {
                                    string taxValue = "";

                                    if (taxCombo.SelectedItem is string taxStr)
                                        taxValue = taxStr;
                                    else if (taxCombo.SelectedItem is ComboBoxItem cbTax)
                                        taxValue = cbTax.Content?.ToString() ?? "";
                                    else
                                        taxValue = taxCombo.Text?.Trim() ?? "";

                                    decimal.TryParse(taxValue.Replace("%", "").Trim(), out tax);
                                }
                                break;
                        }
                    }

                    // Add only valid items
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        items.Add(new QuotationItemModel
                        {
                            Description = description,
                            Hsn = hsn,
                            Uom = uom,
                            Quantity = quantity,
                            Rate = rate,
                            Tax = tax,
                            Amount = quantity * rate * (1 + tax / 100)
                        });
                    }
                }
            }

            return items;
        } 

        private void SetExpiryDateFromQuoteDate()
        {
            DateTime quoteDate;

            // Try to parse the date from the textbox
            if (DateTime.TryParseExact(txtQuoteDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out quoteDate))
            {
                DateTime expiryDate = quoteDate.AddDays(10);
                txtExpiryDate.SelectedDate = expiryDate;
            }
            else
            {
                MessageBox.Show("Invalid quote date format. Please use dd-MM-yyyy.");
            }

        } 

        public void SetupPlaceholder(TextBox textBox, string placeholderText)
        {
            textBox.Text = placeholderText;
            textBox.Foreground = Brushes.Gray;

            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholderText)
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholderText;
                    textBox.Foreground = Brushes.Gray;
                }
            };
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            refresh();
        }

        private bool ValidateQuoteFormInputs()
        {
            // Project selection
            var selectedProject = cmbProject.SelectedItem as ProjectMasterModel;
            if (selectedProject == null || string.IsNullOrWhiteSpace(selectedProject.Project_Name))
            {
                MessageBox.Show("Please select a project.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbProject.Focus();
                return false;
            }

            // Customer selection
            var selectedCustomer = cmbSelectCustomer.SelectedItem as CustomerModel;
            if (selectedCustomer == null || selectedCustomer.CustomerID == 0)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbSelectCustomer.Focus();
                return false;
            }

            // Quote Number
            if (string.IsNullOrWhiteSpace(txtQuoteNumber.Text))
            {
                MessageBox.Show("Please enter the quote number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuoteNumber.Focus();
                return false;
            }

            // Quote Date (readonly but still check)
            if (string.IsNullOrWhiteSpace(txtQuoteDate.Text))
            {
                MessageBox.Show("Quote date is missing.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuoteDate.Focus();
                return false;
            }

            // Expiry Date
            if (!txtExpiryDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select the expiry date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtExpiryDate.Focus();
                return false;
            }

            // Subject
            if (string.IsNullOrWhiteSpace(txtSubject.Text) || txtSubject.Text.Trim() == "Let Your Customer Know What This Quote Is For")
            {
                MessageBox.Show("Please enter a subject for the quote.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSubject.Focus();
                return false;
            }

            return true; // ✅ All validations passed
        }

        private void refresh()
        {



            txtSubject.Text = "";
            txtCustomerNotes.Text = "";
            txtTermsAndConditions.Text = "";
            txtDiscountPercent.Text = "";
            txtDiscountAmount.Text = "";
            txtGrandTotal.Text = "";

            txtStaticItemDescription.Text = "";
            txtStaticItemQty.Text = "";
            txtStaticItemRate.Text = "";
            txtStaticItemAmount.Text = "";
            txtStaticHSN.Text = "";

            cmbSelectCustomer.SelectedIndex = 0;
            cmbProject.SelectedIndex = 0;
            cmbUOM.SelectedIndex = 0;
            cmbStaticItemTax.SelectedIndex = 0;

            txtBillingAddress.Text = "";

            // Remove dynamic item rows
            ItemPanel.Children.Clear();

            // Reset expiry date
            SetExpiryDateFromQuoteDate();
        }

        private void btnViewHistory_Click(object sender, RoutedEventArgs e)
        {
            
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new QuoteHistory(adoHelper));
            }
            else
            {
                MessageBox.Show("MainWindow not found. Are you running in the Designer?");
            }
        }
    }

}
