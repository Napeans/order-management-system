using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using MigraDoc.DocumentObjectModel.Tables;
using cuatomers.DAL;
using System.Windows;
using napeans.dal;

namespace cuatomers
{
    public class GenerateQuote
    {
        private static int quoteCounter = 1;


        public Document CreateQuotationDocument(GenerateQuotationModel model)
        {
            IAdoHelper adoHelper = new AdoHelper();
            ProcessData processData = new ProcessData(adoHelper);
            AppSettingsModel settings = processData.LoadAppSettings();

            if (settings == null)
            {
                MessageBox.Show("App settings not found.");
                return null;
            }

            Document doc = new Document();
            DefineStyles(doc);
            Section section = doc.AddSection();

            // HEADER
            Table headerTable = section.AddTable();
            headerTable.Borders.Width = 0;
            headerTable.AddColumn("5cm");
            headerTable.AddColumn("11cm");
            Row row = headerTable.AddRow();

            if (!string.IsNullOrEmpty(settings.CompanyLogoPath) && File.Exists(settings.CompanyLogoPath))
            {
                Image logo = row.Cells[0].AddImage(settings.CompanyLogoPath);
                logo.LockAspectRatio = true;
                logo.Width = "4cm";
                row.Cells[0].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
            }

            Paragraph info = new Paragraph();
            info.Format.Alignment = ParagraphAlignment.Center;
            info.Format.Font.Size = 9;
            info.AddFormattedText(settings.CompanyName + "\n", TextFormat.Bold);
            info.AddText(settings.CompanyAddress + "\n");
            info.AddText("Contact: " + settings.CompanyPhone + " | ");
            info.AddHyperlink("mailto:" + settings.CompanyEmail, HyperlinkType.Web).AddText(settings.CompanyEmail + "\n");
            info.AddText("GSTIN: " + settings.GSTNumber);
            row.Cells[1].Elements.Add(info);

            // ADDRESS + QUOTE INFO
            section.AddParagraph().Format.SpaceBefore = 10;
            Table infoTable = section.AddTable();
            infoTable.Borders.Width = 0.5;
            infoTable.Format.Font.Size = 9;
            infoTable.AddColumn("6.5cm");
            infoTable.AddColumn("6.5cm");
            infoTable.AddColumn("4cm");
            Row infoRow = infoTable.AddRow();

            Paragraph billTo = new Paragraph();
            billTo.AddFormattedText("BILL TO:\n", TextFormat.Bold);
            foreach (var line in model.BillToAddress.Split('\n')) billTo.AddText(line.Trim() + "\n");
            infoRow.Cells[0].Elements.Add(billTo);

            Paragraph shipTo = new Paragraph();
            shipTo.AddFormattedText("SHIP TO:\n", TextFormat.Bold);
            foreach (var line in model.ShipToAddress.Split('\n')) shipTo.AddText(line.Trim() + "\n");
            infoRow.Cells[1].Elements.Add(shipTo);

            Paragraph quoteInfo = new Paragraph();
            quoteInfo.AddFormattedText("Quotation No: ", TextFormat.Bold);
            quoteInfo.AddText(model.QuoteNumber + "\n");
            quoteInfo.AddFormattedText("Quotation Date: ", TextFormat.Bold);
            quoteInfo.AddText(model.QuoteDate.ToString("dd/MM/yyyy") + "\n");
            quoteInfo.AddFormattedText("Validity Date: ", TextFormat.Bold);
            quoteInfo.AddText(model.ValidityDate.ToString("dd/MM/yyyy") + "\n");
            quoteInfo.AddFormattedText("Sales Contact: ", TextFormat.Bold);
            quoteInfo.AddText(settings.SalesPersonName + "\n");
            quoteInfo.AddFormattedText("Email ID: ", TextFormat.Bold);
            quoteInfo.AddText(settings.CompanyEmail);
            infoRow.Cells[2].Elements.Add(quoteInfo);

            // ITEM TABLE
            section.AddParagraph().Format.SpaceBefore = 10;
            Table table = section.AddTable();
            table.Borders.Width = 0.75;
            table.Format.Font.Size = 9;
            table.AddColumn("1.5cm");
            table.AddColumn("7.0cm");
            table.AddColumn("1.5cm");
            table.AddColumn("1.5cm");
            table.AddColumn("1.5cm");
            table.AddColumn("2.0cm");
            table.AddColumn("2.0cm");

            Row headerRow = table.AddRow();
            string[] headers = { "S.No", "Description", "HSN", "UOM", "Qty", "Unit Rate", "Amount" };
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.Cells[i].AddParagraph().AddFormattedText(headers[i], TextFormat.Bold);
            }

            int index = 1;
            foreach (var item in model.Items)
            {
                Row itemRow = table.AddRow();
                itemRow.Cells[0].AddParagraph(index.ToString());
                itemRow.Cells[1].AddParagraph(item.Description);
                itemRow.Cells[2].AddParagraph(item.Hsn.ToString());
                itemRow.Cells[3].AddParagraph(item.Uom);
                itemRow.Cells[4].AddParagraph(item.Quantity.ToString());
                itemRow.Cells[5].AddParagraph(item.Rate.ToString("N2"));
                itemRow.Cells[6].AddParagraph(item.Amount.ToString("N2"));
                index++;
            }

            Row totalRow = table.AddRow();
            totalRow.Cells[0].MergeRight = 5;
            totalRow.Cells[0].AddParagraph("Total").Format.Alignment = ParagraphAlignment.Right;
            totalRow.Cells[6].AddParagraph(model.TotalAmount.ToString("N2"));

            // Amount in Words
            section.AddParagraph().Format.SpaceBefore = 10;
            string amountInWords = GenerateQuote.ConvertAmountToWords(model.TotalAmount.ToString("N2"));
            Paragraph amountPara = section.AddParagraph();
            amountPara.Format.Font.Bold = true;
            amountPara.Format.SpaceBefore = "0.5cm";
            amountPara.AddText("Amount in Words: ");
            amountPara.AddFormattedText(amountInWords, TextFormat.Italic);

            // Notes & Payment Terms
            section.AddParagraph("\nNotes:", "Bold");
            section.AddParagraph(model.Notes);
            section.AddParagraph("\nPayment Terms:", "Bold");
            section.AddParagraph(model.PaymentTerms);

            // Bank Details
            section.AddParagraph("\nBank Account Details:", "Bold");
            section.AddParagraph("Account Name: " + settings.CompanyName);
            section.AddParagraph("Account No: " + settings.AccountNumber);
            section.AddParagraph("Bank: " + settings.BankName);
            section.AddParagraph("Branch: " + settings.BranchAddress);
            section.AddParagraph("IFSC: " + settings.IFSCCode);

            // Footer
            Paragraph footer = section.Footers.Primary.AddParagraph();
            footer.AddText(settings.CompanyName + ", " + settings.CompanyAddress + "\n");
            footer.AddText("Phone " + settings.CompanyPhone + "   " + settings.CompanyEmail);
            footer.Format.Font.Size = 9;
            footer.Format.Alignment = ParagraphAlignment.Center;

            return doc;
        }
        public void GenerateQuotationPdf(GenerateQuotationModel model)
        {
            var doc = CreateQuotationDocument(model);
            if (doc == null) return;

            var renderer = new PdfDocumentRenderer(true) { Document = doc };
            renderer.RenderDocument();

            string filename = $"Quotation_{model.QuoteNumber}.pdf";
            renderer.PdfDocument.Save(filename);
            Process.Start("explorer.exe", filename);
        }
        public void GenerateQuotationPdfWithDialog(GenerateQuotationModel model)
        {
            var doc = CreateQuotationDocument(model);
            if (doc == null) return;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Quotation_{model.QuoteNumber}.pdf",
                Filter = "PDF files (*.pdf)|*.pdf",
                DefaultExt = ".pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var renderer = new PdfDocumentRenderer(true) { Document = doc };
                renderer.RenderDocument();
                renderer.PdfDocument.Save(saveFileDialog.FileName);
                MessageBox.Show("Quotation saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void DefineStyles(Document doc)
        {
            MigraDoc.DocumentObjectModel.Style style = doc.Styles["Normal"];
            style.Font.Name = "Calibri";
            style.Font.Size = 10;

            MigraDoc.DocumentObjectModel.Style heading1 = doc.Styles.AddStyle("Heading1", "Normal");
            heading1.Font.Bold = true;
            heading1.ParagraphFormat.SpaceAfter = "0.2cm";

            MigraDoc.DocumentObjectModel.Style heading2 = doc.Styles.AddStyle("Heading2", "Normal");
            heading2.Font.Bold = true;
            heading2.Font.Size = 9;

            MigraDoc.DocumentObjectModel.Style bold = doc.Styles.AddStyle("Bold", "Normal");
            bold.Font.Bold = true;
        }

        public static string GenerateQuoteNumber()
        {
            return $"Quote-{quoteCounter++.ToString("D3")}";
        }


        public static string ConvertAmountToWords(string rawAmount)
        {
            if (string.IsNullOrWhiteSpace(rawAmount))
                return "Invalid amount";

            // Clean the input
            string sanitized = rawAmount.Trim()
                .Replace("₹", "")
                .Replace("Rs.", "")
                .Replace("INR", "")
                .Replace(",", "")
                .Trim();

            // Parse to decimal
            if (!decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                return "Invalid amount format";

            if (amount == 0)
                return "Zero Rupees Only";

            long rupees = (long)Math.Floor(amount);
            int paise = (int)((amount - rupees) * 100);

            string amountInWords = $"{NumberToWords(rupees)} Rupees";

            if (paise > 0)
            {
                amountInWords += $" and {NumberToWords(paise)} Paise";
            }

            return amountInWords + " Only";
        }

        private static string NumberToWords(long number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string[] unitsMap = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
                              "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
                              "seventeen", "eighteen", "nineteen" };
            string[] tensMap = { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (!string.IsNullOrEmpty(words))
                    words += "and ";

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }




    }
}

