using cuatomers.DAL;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using napeans.dal;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using MigraDoc.DocumentObjectModel.Shapes;
using Style = MigraDoc.DocumentObjectModel.Style;
namespace cuatomers
{
    public class GenerateDeliveryChallan
    {
        public void GeneratePdf(GenerateDeliveryChallanModel model)
        {
            IAdoHelper adoHelper = new AdoHelper();
            ProcessData processData = new ProcessData(adoHelper);
            AppSettingsModel settings = processData.LoadAppSettings();

            if (settings == null)
            {
                MessageBox.Show("App settings not found.");
                return;
            }

            Document doc = new Document();
            DefineStyles(doc);
            Section section = doc.AddSection();

            // === Header with Logo and Company Info ===
            Table header = section.AddTable();
            header.Borders.Width = 0;
            header.AddColumn("5cm");
            header.AddColumn("11cm");
            Row headerRow = header.AddRow();

            if (!string.IsNullOrEmpty(settings.CompanyLogoPath) && File.Exists(settings.CompanyLogoPath))
            {
                Image logo = headerRow.Cells[0].AddImage(settings.CompanyLogoPath);
                logo.LockAspectRatio = true;
                logo.Width = "4cm";
            }

            Paragraph companyInfo = new Paragraph
            {
                Format = { Font = { Size = 9 }, Alignment = ParagraphAlignment.Center }
            };
            companyInfo.AddFormattedText(settings.CompanyName + "\n", TextFormat.Bold);
            companyInfo.AddText(settings.CompanyAddress + "\n");
            companyInfo.AddText("Contact: " + settings.CompanyPhone + " | ");
            companyInfo.AddHyperlink("mailto:" + settings.CompanyEmail, HyperlinkType.Web).AddText(settings.CompanyEmail + "\n");
            companyInfo.AddText("GSTIN: " + settings.GSTNumber);

            headerRow.Cells[1].Elements.Add(companyInfo);

            section.AddParagraph().Format.SpaceBefore = "0.3cm";

            // === Title ===
            Paragraph title = section.AddParagraph("DELIVERY CHALLAN");
            title.Format.Font.Size = 14;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;

            section.AddParagraph().Format.SpaceBefore = "0.5cm";

            // === Billing, Shipping & Info ===
            Table infoTable = section.AddTable();
            infoTable.Borders.Width = 0.5;
            infoTable.Format.Font.Size = 9;
            infoTable.AddColumn("6.5cm");
            infoTable.AddColumn("6.5cm");
            infoTable.AddColumn("4cm");

            Row infoRow = infoTable.AddRow();

            Paragraph billTo = new Paragraph();
            billTo.AddFormattedText("BILL TO:\n", TextFormat.Bold);
            foreach (var line in model.BillToAddress.Split('\n'))
                billTo.AddText(line.Trim() + "\n");
            infoRow.Cells[0].Elements.Add(billTo);

            Paragraph shipTo = new Paragraph();
            shipTo.AddFormattedText("SHIP TO:\n", TextFormat.Bold);
            foreach (var line in model.ShipToAddress.Split('\n'))
                shipTo.AddText(line.Trim() + "\n");
            infoRow.Cells[1].Elements.Add(shipTo);

            Paragraph challanInfo = new Paragraph();
            challanInfo.AddFormattedText("Challan No: ", TextFormat.Bold);
            challanInfo.AddText(model.ChallanNumber + "\n");
            challanInfo.AddFormattedText("Date: ", TextFormat.Bold);
            challanInfo.AddText(model.ChallanDate.ToString("dd/MM/yyyy"));
            infoRow.Cells[2].Elements.Add(challanInfo);

            section.AddParagraph().Format.SpaceBefore = "0.5cm";

            // === Item Table WITHOUT HSN ===
            Table itemTable = section.AddTable();
            itemTable.Borders.Width = 0.75;
            itemTable.Format.Font.Size = 9;

            itemTable.AddColumn("1.5cm"); // S.No
            itemTable.AddColumn("10cm");  // Description
            itemTable.AddColumn("2.5cm"); // UOM
            itemTable.AddColumn("2.5cm"); // Quantity

            Row headerRow2 = itemTable.AddRow();
            string[] headers = { "S.No", "Description", "UOM", "Qty" };
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow2.Cells[i].AddParagraph().AddFormattedText(headers[i], TextFormat.Bold);
            }

            int index = 1;
            foreach (var item in model.Items)
            {
                Row row = itemTable.AddRow();
                row.Cells[0].AddParagraph(index.ToString());
                row.Cells[1].AddParagraph(item.Description);
                row.Cells[2].AddParagraph(item.Uom ?? "");
                row.Cells[3].AddParagraph(item.Quantity.ToString());
                index++;
            }

            // === Remarks ===
            if (!string.IsNullOrWhiteSpace(model.Remarks))
            {
                section.AddParagraph().Format.SpaceBefore = "0.5cm";
                section.AddParagraph("Remarks:", "Bold");
                section.AddParagraph(model.Remarks);
            }

            // === Signature Section ===
            section.AddParagraph().Format.SpaceBefore = "1cm";
            Table signTable = section.AddTable();
            signTable.AddColumn("7.5cm");
            signTable.AddColumn("7.5cm");

            Row signRow = signTable.AddRow();
            signRow.Cells[0].AddParagraph("Receiver's Signature:\n\n_________________________");
            signRow.Cells[1].AddParagraph("Authorized Signature:\n\n_________________________");

            // === Footer ===
            Paragraph footer = section.Footers.Primary.AddParagraph();
            footer.Format.Font.Size = 9;
            footer.Format.Alignment = ParagraphAlignment.Center;
            footer.AddText($"{settings.CompanyName}, {settings.CompanyAddress}\n");
            footer.AddText($"Phone: {settings.CompanyPhone}   Email: {settings.CompanyEmail}");

            // === Render and Save PDF ===
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true)
            {
                Document = doc
            };
            renderer.RenderDocument();

            string fileName = $"DeliveryChallan_{model.ChallanNumber}.pdf";
            renderer.PdfDocument.Save(fileName);
            Process.Start("explorer.exe", fileName);
        }

        private void DefineStyles(Document doc)
        {
            Style style = doc.Styles["Normal"];
            style.Font.Name = "Calibri";
            style.Font.Size = 10;

            Style heading1 = doc.Styles.AddStyle("Heading1", "Normal");
            heading1.Font.Bold = true;
            heading1.ParagraphFormat.SpaceAfter = "0.2cm";

            Style bold = doc.Styles.AddStyle("Bold", "Normal");
            bold.Font.Bold = true;
        }
    }
}