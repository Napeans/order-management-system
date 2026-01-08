using System;
using System.Collections.Generic;
using System.Linq;

namespace cuatomers
{
    public class Customers
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string GSTIN { get; set; }
        public string Details { get; set; }

    }
   
    public class CustomerModel
    {
        public string Full_Name { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }

        public string ProjectName { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public override string ToString() => CustomerName;  
    }
    public class ProjectMasterModel
    {
        public string Project_Name { get; set; }
        public int Project_ID { get; set; }
        public string CustomerName { get; set; }


    }


    public class SalespersonModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AddedSalesperson { get; set; }

        public string DialogResult { get; set; }

    }
    public class QuotationItemModel
    {
        internal object someInt;

        public int QuotationId { get; set; }
        public int Hsn { get; set; }
        public string Uom { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Rate { get; set; }
        public decimal Tax { get; set; }
        public decimal Amount { get; set; }
        public object SubItems { get; internal set; }
    }
    public class GenerateQuotationModel
    {
        public int QuoteID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string QuoteNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public bool IsActive { get; set; }

        public DateTime QuoteDate { get; set; }
        public DateTime ValidityDate { get; set; } // maps to ExpiryDate

        public string SalesContactName { get; set; } // maps to Salesperson
        public string ProjectName { get; set; }
        public int Hsn { get; set; }
        public string Description { get; set; } // maps to Subject
        public string VendorCode { get; set; }  // not in DB, optional external mapping

        // Address and contact
        public string BillToName { get; set; }
        public string BillToAddress { get; set; }
        public string ShipToName { get; set; }
        public string ShipToAddress { get; set; }

        public string ContactPerson { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string GSTIN { get; set; }

        // Sales contact
        public string SalesContactMobile { get; set; }
        public string SalesContactEmail { get; set; }

        // Financial fields from DB
        public decimal? SubTotal { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string TaxType { get; set; }
        public string TaxName { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? AdjustmentAmount { get; set; }


        public decimal TotalAmount { get; set; }
        public string AmountInWords { get; set; }

        // Notes & terms
        public string PaymentTerms { get; set; }
        public string Notes { get; set; }

        // Additional DB fields
        public string PdfTemplate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }


        // Items
        public List<QuotationItemModel> Items1 { get; set; } = new List<QuotationItemModel>();

      

        // ✅ Add if needed
        public string FullName { get; set; }
        public List<QuotationItemModel> Items { get; set; }
    }

   

    public class FullQuotationModel
    {
        public GenerateQuotationModel Master { get; set; }
        public List<QuotationItemModel> Items { get; set; }
    }



    public class InvoiceItemModel
    {
        internal object someInt;
        public int ItemID { get; set; }
        public int InvoiceId { get; set; }
        public int Hsn { get; set; }
        public string Uom { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        
        public decimal Rate { get; set; }
        public decimal Tax { get; set; }
        public decimal Amount { get; set; }
        public object SubItems { get; internal set; }
    }

    public class GenerateInvoiceModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }

        public int InvoiceId { get; set; }
        public string CustomerID { get; set; }
        public string FullName { get; set; }
          
        public string Description { get; set; }
        public string ProjectName { get; set; }
        public string BillToAddress { get; set; } 
        public string ShipToAddress { get; set; } 
        public string PaymentTerms { get; set; }
        public string Notes { get; set; } 
        public List<InvoiceItemModel> Items { get; set; }

        public decimal TotalAmount => Items.Sum(i => i.Amount);
        public string AmountInWords { get; set; }
    }

    public class FullInvoiceModel
    {
        public GenerateInvoiceModel Master { get; set; }
        public List<InvoiceItemModel> Items { get; set; }
    }

    public class GenerateDeliveryChallanModel
    {
        public string ChallanNumber { get; set; }
        public DateTime ChallanDate { get; set; }
        public string BillToAddress { get; set; }
        public string ShipToAddress { get; set; }
        public string Remarks { get; set; }
        public List<ChallanItemModel> Items { get; set; }
    }

    public class ChallanItemModel
    {
        public string Description { get; set; }
        public string Uom { get; set; }
        public int Quantity { get; set; }
    }

    public class DeliveryChallanItemModel
    {
        public int DeliveryChallanId { get; set; }
        public string Description { get; set; }
        public string Uom { get; set; }
        public int Quantity { get; set; }
    }




    public class ExpenseModel
    {
        public DateTime ExpenseOn { get; set; }
        public string ProjectName { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal ExpenseValue { get; set; }
        public string ModeOfPay { get; set; } = "";
        public string SpentBy { get; set; } = "";
    }

  

    public class AppSettingsModel
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string GSTNumber { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }

        public int PaymentsDueDays { get; set; }
        public string SalesPersonName { get; set; }

        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BranchAddress { get; set; }

        public string CompanyLogoPath { get; set; }
        public string AuthorizedSignPath { get; set; }
    }


    public class InvoicePaymentModel
    {
        public decimal PaidAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool IsSummaryRow { get; set; } = false;
    }

}