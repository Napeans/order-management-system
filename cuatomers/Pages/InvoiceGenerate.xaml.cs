using cuatomers.DAL;
using napeans.dal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cuatomers.Pages
{
    public partial class InvoiceGenerate : Page
    {
        private ObservableCollection<InvoiceItemModel> invoiceItemList = new ObservableCollection<InvoiceItemModel>();

        private readonly IAdoHelper _adoHelper;
        private readonly ProcessData processData;
        private FullInvoiceModel _fullInvoice;
        private bool isEditMode = false;
        private int editingInvoiceId = 0;
        public InvoiceGenerate(IAdoHelper adoHelper, FullInvoiceModel invoiceData)
        {
            InitializeComponent();
            _adoHelper = adoHelper;

            processData = new ProcessData(_adoHelper);
            LoadQuotationComboBox();
            LoadCustomerComboBox();
            LoadProjectComboBox();


            isEditMode = true;
            editingInvoiceId = invoiceData.Master.InvoiceId;

            LoadInvoiceData(invoiceData);
        }
        public InvoiceGenerate(IAdoHelper adoHelper)
        {
            InitializeComponent();
            _adoHelper = adoHelper ?? throw new InvalidOperationException("AdoHelper is null.");
            processData = new ProcessData(_adoHelper);

            LoadQuotationComboBox();
            OnLoad();
        }
        public void OnLoad()
        {
            LoadCustomerComboBox();
            LoadProjectComboBox();
            txtStaticItemQty.TextChanged += StaticRow_TextChanged;
            txtStaticItemRate.TextChanged += StaticRow_TextChanged;
            cmbStaticItemTax.SelectionChanged += StaticRow_TextChanged;
            SetupPlaceholder(txtStaticItemDescription, "Enter A Item.");
            SetupPlaceholder(txtStaticItemQty, "0");
            SetupPlaceholder(txtStaticItemRate, "0");
            SetupPlaceholder(txtSubject, "Let Your Customer Know What This Quote Is For");
            SetupPlaceholder(txtStaticHSN, "HSN");
            txtInvoiceNumber.Text = processData.GenerateNextInvoiceNumber(); ;
            txtInvoiceDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
            SetExpiryDateFromQuoteDate();
            txtDiscountPercent.TextChanged += (s, e) => CalculateGrandTotal();

        }
        private void LoadQuotationComboBox()
        {
            var quotations = processData.GetAllQuotations(); // List<GenerateQuotationModel>

            if (quotations == null || quotations.Count == 0)
                return;

            cmbSelectQuote.ItemsSource = quotations;
            cmbSelectQuote.SelectedIndex = 0; // Optional
        }
        private void cmbSelectQuote_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSelectQuote.SelectedItem is GenerateQuotationModel selectedQuote)
            {
                LoadQuotationIntoInvoice(selectedQuote.QuoteID);
            }
        }
        private void LoadQuotationIntoInvoice(int quoteId)
        {
            var quote = processData.GetQuotationById(quoteId);
            if (quote == null) return;


            var customerDetails = quote.BillToAddress;
            if (customerDetails != null)
            {
                txtBillingAddress.Text = customerDetails;
                txtBillingAddress.Visibility = Visibility.Visible;
                txtblkBilling.Visibility = Visibility.Visible;

            }
            else
            {
                txtBillingAddress.Text = "";
                txtBillingAddress.Visibility = Visibility.Collapsed;
                txtblkBilling.Visibility = Visibility.Visible;
            }

            var customerShippingAddress = quote.ShipToAddress;
            if (customerShippingAddress != null)
            {
                txtShippingAddress.Text = customerDetails;
                txtShippingAddress.Visibility = Visibility.Visible;
                txtShipping.Visibility = Visibility.Visible;

            }
            else
            {
                txtShippingAddress.Text = "";
                txtShippingAddress.Visibility = Visibility.Collapsed;
                txtShipping.Visibility = Visibility.Visible;
            }
            //txtCustomerName.Text = quote.CustomerName;

            txtInvoiceDate.Text = quote.QuoteDate.ToShortDateString();
            txtSubject.Text = quote.Description;
            cmbProject.SelectedValue = quote.ProjectName;
            cmbSelectCustomer.SelectedValue = quote.CustomerName;
            cmbProject.SelectedItem = quote.ProjectName;

            txtDueDate.SelectedDate = quote.ValidityDate;

            txtBillingAddress.Text = quote.BillToAddress;
            txtShippingAddress.Text = quote.ShipToAddress;

            txtTermsAndConditions.Text = quote.PaymentTerms;
            txtCustomerNotes.Text = quote.Notes;



            ItemPanel.Children.Clear();

            foreach (var item in quote.Items)
            {
                AddInvoiceRow(new InvoiceItemModel
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Rate = item.UnitPrice,
                    Tax = item.Tax,
                    Uom = item.Uom,
                    Hsn = item.Hsn
                });
            }



            txtGrandTotal.Text = quote.TotalAmount.ToString("0.00");
        }
        private void AddInvoiceRow(InvoiceItemModel item)
        {
            // Create the row container
            Grid itemGrid = new Grid
            {
                Height = 60,
                Background = Brushes.White,
                Margin = new Thickness(0, 5, 0, 0)
            };

            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // Description
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // HSN
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // UOM
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Qty
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Rate
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Tax
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Amount
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });  // Delete

            // Description
            TextBox txtItemDescription = new TextBox
            {
                Margin = new Thickness(20, 0, 0, 0),
                Style = (Style)FindResource("BorderlessTextBox")
            };

            // ✅ Set only if description is present
            if (!string.IsNullOrWhiteSpace(item.Description))
                txtItemDescription.Text = item.Description;
            else
                SetupPlaceholder(txtItemDescription, "Enter A Item.");  // only for empty field

            Grid.SetColumn(txtItemDescription, 0);
            itemGrid.Children.Add(txtItemDescription);

            // HSN
            TextBox txtHSN = new TextBox
            {
                Width = 90,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            if (!string.IsNullOrWhiteSpace(item.Hsn.ToString()))
                txtHSN.Text = item.Hsn.ToString();
            else
                SetupPlaceholder(txtHSN, "HSN");
            Grid.SetColumn(txtHSN, 1);
            itemGrid.Children.Add(txtHSN);

            // UOM
            ComboBox cmbUOM = new ComboBox
            {
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = (Style)FindResource("BorderlessComboBox")
            };
            string[] uoms = { "PCS", "KG", "Litre", "Meter", "Box" };
            foreach (var u in uoms)
                cmbUOM.Items.Add(u);

            // ✅ Set only if it's a valid UOM
            cmbUOM.SelectedItem = uoms.Contains(item.Uom) ? item.Uom : null;

            Grid.SetColumn(cmbUOM, 2);
            itemGrid.Children.Add(cmbUOM);

            // Quantity
            TextBox txtItemQty = new TextBox
            {
                Width = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            txtItemQty.Text = item.Quantity > 0 ? item.Quantity.ToString() : "";
            if (item.Quantity <= 0)
                SetupPlaceholder(txtItemQty, "0");
            Grid.SetColumn(txtItemQty, 3);
            itemGrid.Children.Add(txtItemQty);

            // Rate
            TextBox txtItemRate = new TextBox
            {
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            txtItemRate.Text = item.Rate > 0 ? item.Rate.ToString("0.##") : "";
            if (item.Rate <= 0)
                SetupPlaceholder(txtItemRate, "0");
            Grid.SetColumn(txtItemRate, 4);
            itemGrid.Children.Add(txtItemRate);

            // Tax
            ComboBox cmbItemTax = new ComboBox
            {
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = (Style)FindResource("BorderlessComboBox")
            };
            string[] taxes = { "0%", "5%", "12%", "18%", "28%" };
            foreach (var t in taxes)
                cmbItemTax.Items.Add(t);

            // ✅ Set only if matches one of the predefined tax options
            string taxValue = $"{item.Tax}%";
            cmbItemTax.SelectedItem = taxes.Contains(taxValue) ? taxValue : "0%";

            Grid.SetColumn(cmbItemTax, 5);
            itemGrid.Children.Add(cmbItemTax);

            // Amount
            decimal amount = item.Quantity > 0 && item.Rate > 0
                ? item.Quantity * item.Rate * (1 + item.Tax / 100)
                : 0;

            TextBlock txtItemAmount = new TextBlock
            {
                Text = amount > 0 ? amount.ToString("0.00") : "",
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(txtItemAmount, 6);
            itemGrid.Children.Add(txtItemAmount);


            // Recalculate on input
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

            // Delete button
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
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            buttonPanel.Children.Add(btnRemove);
            Grid.SetColumn(buttonPanel, 7);
            itemGrid.Children.Add(buttonPanel);

            ItemPanel.Children.Add(itemGrid);
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
        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {


            Grid itemGrid = new Grid
            {
                Height = 60,
                Background = Brushes.White,
                Margin = new Thickness(0, 5, 0, 0)
            };

            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // Description TextBox
            TextBox txtItemDescription = new TextBox
            {
                Text = "Type or click to select an item.",
                Margin = new Thickness(20, 0, 0, 0),
                Style = (Style)FindResource("BorderlessTextBox")
            };
            Grid.SetColumn(txtItemDescription, 0);
            itemGrid.Children.Add(txtItemDescription);

            // Quantity TextBox
            TextBox txtItemQty = new TextBox
            {
                Text = "1.00",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            Grid.SetColumn(txtItemQty, 1);
            itemGrid.Children.Add(txtItemQty);

            // Rate TextBox
            TextBox txtItemRate = new TextBox
            {
                Text = "0.00",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)FindResource("BorderlessTextBox")
            };
            Grid.SetColumn(txtItemRate, 2);
            itemGrid.Children.Add(txtItemRate);

            // Tax ComboBox
            ComboBox cmbItemTax = new ComboBox
            {
                Text = "Select a Tax",
                Width = 140,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = (Style)FindResource("BorderlessComboBox")
            };
            Grid.SetColumn(cmbItemTax, 3);
            itemGrid.Children.Add(cmbItemTax);

            // Amount TextBlock
            TextBlock txtItemAmount = new TextBlock
            {
                Text = "0.00",
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(txtItemAmount, 4);
            itemGrid.Children.Add(txtItemAmount);

            // Action buttons panel
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            //Button btnMenu = new Button
            //{
            //    Content = "⋮",
            //    Background = Brushes.Transparent,
            //    BorderThickness = new Thickness(0),
            //    Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
            //    FontSize = 16,
            //    Margin = new Thickness(0, 0, 5, 0)
            //};

            Button btnRemove = new Button
            {
                Content = "✕",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 87, 34)),
                FontSize = 14
            };

            // ✅ Remove this specific row when clicked
            btnRemove.Click += (s, ev) => ItemPanel.Children.Remove(itemGrid);

            //buttonPanel.Children.Add(btnMenu);
            buttonPanel.Children.Add(btnRemove);
            Grid.SetColumn(buttonPanel, 5);
            itemGrid.Children.Add(buttonPanel);

            // Add to top of list
            ItemPanel.Children.Insert(0, itemGrid);
        }
        private void StaticRow_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtStaticItemQty.Text, out decimal qty) &&
                decimal.TryParse(txtStaticItemRate.Text, out decimal rate))
            {
                decimal taxRate = 0;

                // 👇 Handle ComboBoxItem properly
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
        private void SetExpiryDateFromQuoteDate()
        {
            DateTime quoteDate;

            // Try to parse the date from the textbox
            if (DateTime.TryParseExact(txtInvoiceDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out quoteDate))
            {
                DateTime expiryDate = quoteDate.AddDays(10);
                txtDueDate.SelectedDate = expiryDate;
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

        private void btnSaveAndSendInvoice_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Traditional null and type check
            if (cmbSelectCustomer == null || cmbSelectCustomer.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedCustomer = cmbSelectCustomer.SelectedItem as CustomerModel;
            if (selectedCustomer == null)
            {
                MessageBox.Show("Invalid customer selection.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int customerId = selectedCustomer.CustomerID;
            string selectedCustomerName = selectedCustomer.Full_Name?.Trim() ?? "";



            string invoiceNumber = txtInvoiceNumber?.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                MessageBox.Show("Invoice number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime invoiceDate = DateTime.TryParse(txtInvoiceDate?.Text, out var dt) ? dt : DateTime.Today;
            DateTime dueDate = txtDueDate?.SelectedDate ?? DateTime.Today;

            string projectName = cmbProject?.Text ?? "";
            string subject = txtSubject?.Text ?? "";
            decimal discountPercent = decimal.TryParse(txtDiscountPercent?.Text, out var dp) ? dp : 0;
            string taxType = cmbStaticItemTax?.SelectedItem?.ToString() ?? "";
            decimal grandTotal = decimal.TryParse(txtGrandTotal?.Text, out var gt) ? gt : 0;
            string customerNotes = txtCustomerNotes?.Text ?? "";
            string termsAndConditions = txtTermsAndConditions?.Text ?? "";

            int invoiceId = -1;

            if (isEditMode && editingInvoiceId > 0)
            {

                bool updated = processData.UpdateInvoice(
                    editingInvoiceId,
                    customerId,
                    invoiceNumber,
                    invoiceDate,
                    dueDate,
                    projectName,
                    subject,
                    discountPercent,
                    taxType,
                    grandTotal,
                    customerNotes,
                    termsAndConditions
                );

                if (!updated)
                {
                    MessageBox.Show("Failed to update invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                invoiceId = editingInvoiceId;
            }
            else
            {

                invoiceId = processData.AddNewInvoice(
                    customerId,
                    invoiceNumber,
                    invoiceDate,
                    dueDate,
                    projectName,
                    subject,
                    discountPercent,
                    taxType,
                    grandTotal,
                    customerNotes,
                    termsAndConditions
                );

                if (invoiceId <= 0)
                {
                    MessageBox.Show("Failed to save invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // 💾 Save invoice items
            bool itemsSaved = ExtractAndSaveAllInvoiceItems(invoiceId);

            if (itemsSaved)
            {
                MessageBox.Show(isEditMode ? "Invoice updated successfully!" : "Invoice saved successfully!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Invoice saved, but item saving failed.", "Partial Failure", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // ✅ Reset state
            isEditMode = false;
            editingInvoiceId = 0;
        }


        private bool ExtractAndSaveAllInvoiceItems(int invoiceId)
        {
            // ❗ Delete existing items first (only in edit mode)
            if (isEditMode)
                processData.DeleteInvoiceItems(invoiceId);

            var items = new List<InvoiceItemModel>();

            // 1. STATIC ITEM
            string desc = txtStaticItemDescription.Text?.Trim();
            int qty = int.TryParse(txtStaticItemQty.Text, out int q1) ? q1 : 0;
            decimal rate = decimal.TryParse(txtStaticItemRate.Text, out decimal r1) ? r1 : 0;
            decimal hsn = decimal.TryParse(txtStaticHSN.Text, out decimal h1) ? h1 : 0;
            decimal tax = 0;
            string uom = GetComboBoxValue(cmbUOM);

            if (cmbStaticItemTax.SelectedItem is ComboBoxItem staticTaxItem)
            {
                decimal.TryParse(staticTaxItem.Content.ToString().Replace("%", ""), out tax);
            }

            // Save static item only if ItemPanel has no children (no dynamic rows)
            if (ItemPanel.Children.Count == 0 && !string.IsNullOrWhiteSpace(desc))
            {
                items.Add(new InvoiceItemModel
                {
                    InvoiceId = invoiceId,
                    Description = desc,
                    Quantity = qty,
                    Rate = rate,
                    Tax = tax,
                    Hsn = Convert.ToInt32(hsn),
                    Uom = string.IsNullOrWhiteSpace(uom) ? "PCS" : uom,
                    Amount = qty * rate + (qty * rate * tax / 100)
                });
            }


            foreach (UIElement row in ItemPanel.Children)
            {
                if (row is Grid grid && grid.Children.Count >= 6)
                {
                    string dDesc = "";
                    int dQty = 0, dHsn = 0;
                    decimal dRate = 0, dTax = 0;
                    string dUom = "";

                    if (grid.Children[0] is TextBox descBox)
                        dDesc = descBox.Text?.Trim();

                    if (grid.Children[1] is TextBox hsnBox)
                        int.TryParse(hsnBox.Text, out dHsn);

                    if (grid.Children[2] is ComboBox uomCombo)
                        dUom = GetComboBoxValue(uomCombo);

                    if (grid.Children[3] is TextBox qtyBox)
                        int.TryParse(qtyBox.Text, out dQty);

                    if (grid.Children[4] is TextBox rateBox)
                        decimal.TryParse(rateBox.Text, out dRate);

                    if (grid.Children[5] is ComboBox taxCombo && taxCombo.SelectedItem != null)
                    {
                        string taxStr = GetComboBoxValue(taxCombo);
                        decimal.TryParse(taxStr.Replace("%", ""), out dTax);
                    }

                    if (!string.IsNullOrWhiteSpace(dDesc))
                    {
                        items.Add(new InvoiceItemModel
                        {
                            InvoiceId = invoiceId,
                            Description = dDesc,
                            Quantity = dQty,
                            Rate = dRate,
                            Tax = dTax,
                            Hsn = dHsn,
                            Uom = string.IsNullOrWhiteSpace(dUom) ? "PCS" : dUom,
                            Amount = dQty * dRate + (dQty * dRate * dTax / 100)
                        });
                    }
                }
            }

            // 3. REMOVE DUPLICATES (optional)
            items = items
                .GroupBy(x => new { x.Description, x.Quantity, x.Rate, x.Tax, x.Hsn, x.Uom })
                .Select(g => g.First())
                .ToList();

            // 4. SAVE TO DATABASE
            foreach (var item in items)
            {
                bool saved = processData.AddInvoiceItem(
                    item.InvoiceId,
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
        private string GetComboBoxValue(ComboBox combo)
        {
            if (combo == null) return "";

            if (combo.SelectedItem is string str)
                return str.Trim();

            if (combo.SelectedItem is ComboBoxItem cbItem)
                return cbItem.Content?.ToString()?.Trim() ?? "";

            return combo.Text?.Trim() ?? "";
        }

        private void LoadProjectComboBox()
        {
            try
            {
                var projectList = processData.GetProjectName(); // Your method

                projectList.Insert(0, new ProjectMasterModel
                {
                    Project_ID = 0,
                    Project_Name = "-- Select Project --"

                });

                cmbProject.ItemsSource = projectList;
                cmbProject.DisplayMemberPath = "Project_Name";
                cmbProject.SelectedValuePath = "Project_Name"; // or GSTIN_No or another unique ID if needed

                cmbProject.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Project Name: " + ex.Message);
            }
        }

        private void LoadCustomerComboBox()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load customers: " + ex.Message);
            }
        }

        private void btnInvoiceHistory_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new InvoiceHistory(adoHelper));
            }
            else
            {
                MessageBox.Show("MainWindow not found. Are you running in the Designer?");
            }
        }

        public void LoadInvoiceData(FullInvoiceModel data)
        {
            if (data == null || data.Master == null)
                return;

            // Basic fields
            //cmbSelectQuote.SelectedValue = data.Master.InvoiceNumber;
            txtInvoiceNumber.Text = data.Master.InvoiceNumber;
            txtInvoiceDate.Text = data.Master.InvoiceDate.ToString("dd-MM-yyyy");
            txtSubject.Text = data.Master.Description ?? "";
            txtCustomerNotes.Text = data.Master.Notes ?? "";
            txtGrandTotal.Text = data.Master.TotalAmount.ToString("0.00");
            txtDueDate.SelectedDate = data.Master.DueDate;

            // Set customer ComboBox (Full_Name is used as SelectedValuePath)
            if (cmbSelectCustomer != null && !string.IsNullOrWhiteSpace(data.Master.FullName))
            {
                cmbSelectCustomer.SelectedValue = data.Master.FullName;
            }

            // Set project ComboBox (assuming you use Project_Name as SelectedValuePath)
            if (cmbProject != null && !string.IsNullOrWhiteSpace(data.Master.ProjectName))
            {
                cmbProject.SelectedValue = data.Master.ProjectName;
            }


            // If you have payment terms, pan, etc.
            txtTermsAndConditions.Text = data.Master.PaymentTerms ?? "";
            //txtPan.Text = data.Master.PanNumber ?? "";

            // Clear and repopulate item rows
            ItemPanel.Children.Clear();
            foreach (var item in data.Items)
            {
                AddInvoiceItemRow(item);
            }

            // Final calculation
            CalculateGrandTotal();
        }

        private void AddInvoiceItemRow(InvoiceItemModel item)
        {
            Grid itemGrid = new Grid
            {
                Height = 60,
                Background = Brushes.White,
                Margin = new Thickness(0, 5, 0, 0)
            };

            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // Description
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // HSN
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // UOM
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Qty
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Rate
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Tax
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Amount
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });  // Delete

            // Description
            TextBox txtItemDescription = new TextBox
            {
                Text = item.Description,
                Style = (Style)FindResource("BorderlessTextBox"),
                Margin = new Thickness(20, 0, 0, 0)
            };
            Grid.SetColumn(txtItemDescription, 0);
            itemGrid.Children.Add(txtItemDescription);

            // HSN
            TextBox txtHSN = new TextBox
            {
                Text = item.Hsn.ToString(),
                Style = (Style)FindResource("BorderlessTextBox"),
                Width = 90,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtHSN, 1);
            itemGrid.Children.Add(txtHSN);

            // UOM
            ComboBox cmbUOM = new ComboBox
            {
                Style = (Style)FindResource("BorderlessComboBox"),
                Width = 80,
                Name = "cmbUOM"
            };

            // Add standard UOMs
            string[] uoms = { "PCS", "KG", "Litre", "Meter", "Box" };
            foreach (var u in uoms)
            {
                if (!cmbUOM.Items.Contains(u))
                    cmbUOM.Items.Add(u);
            }

            // Ensure UOM is present and selected
            if (!string.IsNullOrWhiteSpace(item.Uom))
            {
                if (!cmbUOM.Items.Contains(item.Uom))
                    cmbUOM.Items.Add(item.Uom);

                cmbUOM.SelectedItem = item.Uom;
            }

            Grid.SetColumn(cmbUOM, 2);
            itemGrid.Children.Add(cmbUOM);

            // Quantity
            TextBox txtItemQty = new TextBox
            {
                Text = item.Quantity.ToString(),
                Style = (Style)FindResource("BorderlessTextBox"),
                Width = 60,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtItemQty, 3);
            itemGrid.Children.Add(txtItemQty);

            // Rate
            TextBox txtItemRate = new TextBox
            {
                Text = item.Rate.ToString("0.00"),
                Style = (Style)FindResource("BorderlessTextBox"),
                Width = 80,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(txtItemRate, 4);
            itemGrid.Children.Add(txtItemRate);

            // Tax
            ComboBox cmbItemTax = new ComboBox
            {
                Style = (Style)FindResource("BorderlessComboBox"),
                Width = 100
            };

            string[] taxRates = { "0%", "5%", "12%", "18%", "28%" };
            foreach (var rate in taxRates)
            {
                if (!cmbItemTax.Items.Contains(rate))
                    cmbItemTax.Items.Add(rate);
            }

            // Format the tax value to match list
            string formattedTax = item.Tax % 1 == 0
                ? ((int)item.Tax).ToString() + "%"
                : item.Tax.ToString("0.##") + "%";

            if (!cmbItemTax.Items.Contains(formattedTax))
                cmbItemTax.Items.Add(formattedTax);

            cmbItemTax.SelectedItem = formattedTax;

            Grid.SetColumn(cmbItemTax, 5);
            itemGrid.Children.Add(cmbItemTax);

            // Amount
            decimal totalAmount = item.Quantity * item.Rate * (1 + item.Tax / 100);
            TextBlock txtItemAmount = new TextBlock
            {
                Text = totalAmount.ToString("0.00"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium
            };
            Grid.SetColumn(txtItemAmount, 6);
            itemGrid.Children.Add(txtItemAmount);

            // Recalculate on changes
            txtItemQty.TextChanged += (s, ev) => { UpdateRowAmount(txtItemQty, txtItemRate, cmbItemTax, txtItemAmount); CalculateGrandTotal(); };
            txtItemRate.TextChanged += (s, ev) => { UpdateRowAmount(txtItemQty, txtItemRate, cmbItemTax, txtItemAmount); CalculateGrandTotal(); };
            cmbItemTax.SelectionChanged += (s, ev) => { UpdateRowAmount(txtItemQty, txtItemRate, cmbItemTax, txtItemAmount); CalculateGrandTotal(); };

            // Delete Button
            Button btnRemove = new Button
            {
                Content = "✕",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 87, 34)),
                FontSize = 14
            };
            btnRemove.Click += (s, e) =>
            {
                if (MessageBox.Show("Remove this item?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ItemPanel.Children.Remove(itemGrid);
                    CalculateGrandTotal();
                }
            };

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            buttonPanel.Children.Add(btnRemove);
            Grid.SetColumn(buttonPanel, 7);
            itemGrid.Children.Add(buttonPanel);

            // Add to main panel
            ItemPanel.Children.Add(itemGrid);
        }




    }
}
