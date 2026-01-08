using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using cuatomers.DAL;         //   IAdoHelper
using napeans.dal;          //   ProcessData & your models
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.IO;
using System.Diagnostics;
namespace cuatomers.Pages
{
    /// <summary>
    /// Interaction logic for ExpenceHistry.xaml
    public partial class ExpenceHistry : Page
    {
        private readonly ProcessData _processData;

        public ExpenceHistry(IAdoHelper adoHelper)
        {
            InitializeComponent();
            _processData = new ProcessData(adoHelper);
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                List<ExpenseModel> history = _processData.GetAllExpenses();
                HistoryGrid.ItemsSource = history;

                if (history?.Count > 0)
                {
                    HistoryGrid.SelectedIndex = 0;
                    HistoryGrid.ScrollIntoView(HistoryGrid.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load history: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;

                if (startDate == null || endDate == null)
                {
                    MessageBox.Show("Please select both start and end dates.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (startDate > endDate)
                {
                    MessageBox.Show("Start date cannot be greater than end date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Call filtered history
                List<ExpenseModel> filtered = _processData.GetExpensesByDateRange((DateTime)startDate, (DateTime)endDate);

                HistoryGrid.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to filter data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void home_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new ExpensePage(adoHelper));

            }
            else
            {
                MessageBox.Show("ProjectMaster not found. Are you running in the Designer?");
            }
        }


        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            // 1 — Grab the items from the grid
            var items = HistoryGrid.ItemsSource as IEnumerable<ExpenseModel>;
            if (items == null || !items.Any())
            {
                MessageBox.Show("No data to print.");
                return;
            }

            // 2 — Create a new MigraDoc document
            var document = new Document();
            var section = document.AddSection();
            section.PageSetup.LeftMargin = Unit.FromCentimeter(0.5);  // or 0 cm for no space
            section.PageSetup.RightMargin = Unit.FromCentimeter(0.5); // keep some right margin
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);

            // 3 — Add a title
            var title = section.AddParagraph("Expense History");
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.SpaceAfter = 20;
            title.Format.Alignment = ParagraphAlignment.Center;

            // 4 — Create the table
            var table = section.AddTable();
            table.Borders.Width = 0.75;
            table.Format.Font.Size = 12;
            table.Format.Alignment = ParagraphAlignment.Left;
            table.Rows.LeftIndent = 0;
            

            // Set column widths
            table.AddColumn(Unit.FromCentimeter(2.5)); // Date
            table.AddColumn(Unit.FromCentimeter(3.5)); // Project
            table.AddColumn(Unit.FromCentimeter(4.5)); // Description
            table.AddColumn(Unit.FromCentimeter(3));   // Amount
            table.AddColumn(Unit.FromCentimeter(3));   // Mode
            table.AddColumn(Unit.FromCentimeter(3.5));   // Spent By

            string[] headers = { "Date", "Project", "Description", "Amount", "Mode", "Spent By" };
            double[] columnWidths = { 2.5, 3.5, 4.5, 3, 3, 3.5 };
            foreach (var _ in headers)
                table.AddColumn(Unit.FromCentimeter(3));   // 6 × 3 cm  → 18 cm usable width (A4 portrait)

            // 4a — Header row
            var headerRow = table.AddRow();
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.Cells[i].AddParagraph(headers[i]);
                headerRow.Cells[i].Format.Font.Bold = true;
                headerRow.Cells[i].Format.Alignment = ParagraphAlignment.Center;
                headerRow.Cells[i].Shading.Color = Colors.LightBlue;
            }

            // 4b — Data rows
            foreach (var item in items)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(item.ExpenseOn.ToString("dd-MM-yyyy"));
                row.Cells[1].AddParagraph(item.ProjectName ?? "");
                row.Cells[2].AddParagraph(item.Description ?? "");
                row.Cells[3].AddParagraph(item.ExpenseValue.ToString("F2"));
                row.Cells[4].AddParagraph(item.ModeOfPay ?? "");
                row.Cells[5].AddParagraph(item.SpentBy ?? "");
            }

            // 5 — Render and save
            var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
            renderer.RenderDocument();

            string filename = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "ExpenseHistory.pdf");

            renderer.PdfDocument.Save(filename);

            // 6 — Open the PDF
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }
    }
    }

