using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Xml.Linq;
using cuatomers.Pages;
using napeans.dal;

namespace cuatomers.DAL
{
    public class ProcessData
    {
        private readonly IAdoHelper _adoHelper;

        public ProcessData(IAdoHelper adoHelper)
        {
            _adoHelper = adoHelper ?? throw new InvalidOperationException("AdoHelper is not initialized.");
        }


        //Customers
        public bool AddNewCustomer(string saulition, string firstname, string lastname, String gstNumber, string companyname, string email, string contact,
            string attention, string country, string addressLine1, string addressLine2, string city, string state, string pinCode, string phone, string fax,
           string shattention, string shcountry, string shaddressLine1, string shaddressLine2, string shcity, string shstate, String shpinCode, string shphone, string shpfax,
           String OpeningBalance, string PaymentTerms, string PortalLanguage, String pan, string currency)
        {
            string query = "INSERT INTO Customers (Saulution, First_Name, Last_Name,GST_Number, Company_Name,  Email, Contact, Bn_Line1, Bn_country, Bn_address1, Bn_address2, " +
                "Bn_city, bn_State, Bn_pincode, Bn_Phone, Bn_faxNumber, sh_attention,sh_country, sh_address1, sh_address2," +
                "sh_city, sh_state, sh_pincode, sh_phone, sh_faxNumber, opening_balance, payment_terms, portal_language, pan, currency) " +


                "VALUES (@saulition, @firstname,  @lastname, @gstNumber, @companyname,  @email, @contact, @attention, @country, @addressLine1, @addressLine2 , @city," +
                " @state, @pinCode , @phone, @fax, @shattention, @shcountry, @shaddressLine1,@shaddressLine2, @shcity, @shstate, @shpinCode,@shphone, @shpfax," +
                " @OpeningBalance, @PaymentTerms, @PortalLanguage, @pan, @currency  )";


            var parms = new Dictionary<string, string>();
            parms.Add("@saulition", saulition);
            parms.Add("@firstname", firstname);
            parms.Add("@lastname", lastname);
            parms.Add("@companyname", companyname);
            parms.Add("@gstNumber", gstNumber);
            parms.Add("@email", email);
            parms.Add("@contact", contact);
            parms.Add("@attention", attention);
            parms.Add("@country", country);
            parms.Add("@addressLine1", addressLine1);
            parms.Add("@addressLine2", addressLine2);
            parms.Add("@city", city);
            parms.Add("@state", state);
            parms.Add("@pinCode", pinCode);
            parms.Add("@phone", phone);
            parms.Add("@fax", fax);
            parms.Add("@shattention", shattention);
            parms.Add("@shcountry", shcountry);
            parms.Add("@shaddressLine1", shaddressLine1);
            parms.Add("@shaddressLine2", shaddressLine2);
            parms.Add("@shcity", shcity);
            parms.Add("@shstate", shstate);
            parms.Add("@shpinCode", shpinCode);
            parms.Add("@shphone", shphone);
            parms.Add("@shpfax", shpfax);
            parms.Add("@OpeningBalance", OpeningBalance);
            parms.Add("@PaymentTerms", PaymentTerms);
            parms.Add("@PortalLanguage", PortalLanguage);
            parms.Add("@pan", pan);
            parms.Add("@currency", currency);
            int rowsAffected = _adoHelper.ExecNonQuery(query, parms);
            return rowsAffected > 0;
        }

         
        public List<CustomerModel> GetCustomers()
        {
            var customers = new List<CustomerModel>();

            if (_adoHelper == null)
                throw new InvalidOperationException("AdoHelper is not initialized.");

            string query = "SELECT CustomerID, First_Name, Last_Name FROM Customers";

            var ds = _adoHelper.ExecDataSet(query, null as List<SqlParameter>);

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0] == null)
                return customers;  

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var firstName = row["First_Name"]?.ToString() ?? "";
                var lastName = row["Last_Name"]?.ToString() ?? "";

                customers.Add(new CustomerModel
                {
                    CustomerID = Convert.ToInt32(row["CustomerID"]),
                    Full_Name = $"{firstName} {lastName}".Trim()
                });
            }

            return customers;
        } 

        public CustomerModel GetCustomerDetailsByFullName(string fullName)
        {
            string query = @"
        SELECT CustomerID, First_Name, Last_Name, Bn_address1, Bn_address2, Bn_city, Bn_State, Bn_pincode, Bn_Phone
        FROM Customers
        WHERE CONCAT(First_Name, ' ', Last_Name) = @FullName";


            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@FullName", fullName)
    };

            var ds = _adoHelper.ExecDataSet(query, parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];

                string address = $"{row["Bn_address1"]}\n{row["Bn_address2"]}\n{row["Bn_city"]}\n{row["Bn_State"]}\n{row["Bn_pincode"]} {row["Bn_Phone"]}";
                int customerID = Convert.ToInt32(row["CustomerID"]);

                return new CustomerModel
                {
                    Full_Name = fullName,
                    BillingAddress = address,
                    CustomerID = customerID
                };
            }

            return null;
        }

        public CustomerModel GetCustomerShippingAddress(string fullName)
        {
            string query = @"
        SELECT CustomerID, First_Name, Last_Name, sh_address1, sh_address2, sh_city, sh_State, sh_pincode, sh_Phone
        FROM Customers
        WHERE CONCAT(First_Name, ' ', Last_Name) = @FullName";

            // Use List<SqlParameter> instead of Dictionary
            var parameters = new List<SqlParameter>
            {
        new SqlParameter("@FullName", fullName)
            };

            var ds = _adoHelper.ExecDataSet(query, parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];

                string address = $"{row["sh_address1"]}\n{row["sh_address2"]}\n{row["sh_city"]}\n{row["sh_State"]}\n{row["sh_pincode"]}\n {row["sh_Phone"]}";
                int customerID = Convert.ToInt32(row["CustomerID"]);

                return new CustomerModel
                {
                    Full_Name = fullName,
                    BillingAddress = address,
                    CustomerID = customerID
                };
            }

            return null;
        }



        //Project
        public bool AddNewProject(
     string projectName,
     string description,
     DateTime startDate,
     DateTime? endDate,
     string contactPerson,
     string contactNumber,
     string department,
     string customerMaster)
        {
            string query = @"
        INSERT INTO ProjectsMaster 
        (ProjectName, Description, StartDate, ApproximateEndDate, ContactPerson, ContactNumber, Department, CustomerMaster) 
        VALUES 
        (@ProjectName, @Description, @StartDate, @EndDate, @ContactPerson, @ContactNumber, @Department, @CustomerMaster)";

            var parms = new Dictionary<string, string>
    {
        { "@ProjectName", projectName },
        { "@Description", description },
        { "@StartDate", startDate.ToString("yyyy-MM-dd") },
        { "@EndDate", endDate?.ToString("yyyy-MM-dd") ?? string.Empty },  // Let ExecNonQuery handle empty
        { "@ContactPerson", contactPerson },
        { "@ContactNumber", contactNumber },
        { "@Department", department },
        { "@CustomerMaster", customerMaster }  // ✅ Must exist here
    };

            return _adoHelper.ExecNonQuery(query, parms) > 0;
        }

        public List<ProjectMasterModel> GetProjectName()
        {
            string query = "SELECT ProjectID, ProjectName, CustomerMaster FROM ProjectsMaster";
            var projectMaster = new List<ProjectMasterModel>();

            // Pass null with correct type: List<SqlParameter>
            var ds = _adoHelper.ExecDataSet(query, null as List<SqlParameter>);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    projectMaster.Add(new ProjectMasterModel
                    {
                        Project_ID = Convert.ToInt32(row["ProjectID"]),
                        Project_Name = row["ProjectName"].ToString(),
                        CustomerName = row["CustomerMaster"].ToString()
                    });
                }
            }

            return projectMaster;
        }

        public List<CustomerModel> GetProjectNameByCustomer()
        {
            var customers = new List<CustomerModel>();  

            if (_adoHelper == null)
                throw new InvalidOperationException("AdoHelper is not initialized.");

            string query = @"
       SELECT 
    c.CustomerID,
    c.First_Name,
    c.Last_Name,
    p.ProjectName
FROM Customers c
LEFT JOIN ProjectsMaster p 
    ON (RTRIM(LTRIM(c.First_Name + ' ' + c.Last_Name)) = RTRIM(LTRIM(p.CustomerMaster)))";  

            var ds = _adoHelper.ExecDataSet(query, null as List<SqlParameter>);

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0] == null)
                return customers;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var firstName = row["First_Name"]?.ToString() ?? "";
                var lastName = row["Last_Name"]?.ToString() ?? "";
                var projectName = row["ProjectName"]?.ToString() ?? "";

                customers.Add(new CustomerModel
                {
                    CustomerID = Convert.ToInt32(row["CustomerID"]),
                    Full_Name = $"{firstName} {lastName}".Trim(),
                    ProjectName = projectName
                });
            }

            return customers;
        }


        //Expense
        public bool AddExpenseProject(string projectName, string otherExpense, string description, string modeofPay, string expenseValue, string spentby, DateTime Exdate)
        {
            try
            {
                if (!decimal.TryParse(expenseValue, out decimal parsedExpenseValue))
                {
                    MessageBox.Show("Invalid Expense Value. Please enter a valid number.");
                    return false;
                }

                string query = @"INSERT INTO ExpenseManager 
                         (Project_Name,  Description, ModeOf_Pay, Expense_Value, Spent_By, Expense_Date,  Other_Expense) 
                         VALUES 
                         (@Project_Name, @Description, @ModeOf_Pay, @Expense_Value, @Spent_By, @Expense_Date, @Other_Expense)";

                var parms = new Dictionary<string, string>
                {
                    {"@Project_Name", projectName},

                    {"@Description", description},
                    {"@ModeOf_Pay", modeofPay},
                    {"@Expense_Value", parsedExpenseValue.ToString() },
                    {"@Spent_By", spentby},
                    {"@Expense_Date", Exdate.ToString("yyyy-MM-dd")},
                      {"@Other_Expense", otherExpense },
                };

                int rowsAffected = _adoHelper.ExecNonQuery(query, parms);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
       
        public List<ExpenseModel> GetAllExpenses()
        {

            const string sql = @"
        SELECT  
            Expense_Date     AS ExpenseOn,
            Other_Expense    AS ProjectName,
            Description,
            ModeOf_Pay       AS ModeOfPay,
            Expense_Value    AS ExpenseValue,
            Spent_By         AS SpentBy
        FROM ExpenseManager
        ORDER BY Expense_Date DESC";

            // Make sure ExecDataSet returns DataSet. Cast if needed.
            var ds = _adoHelper.ExecDataSet(sql, null) as DataSet;

            var list = new List<ExpenseModel>();

            try
            {
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable table = ds.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        list.Add(new ExpenseModel
                        {
                            ExpenseOn = row.Table.Columns.Contains("ExpenseOn") ? row.Field<DateTime?>("ExpenseOn") ?? DateTime.MinValue : DateTime.MinValue,
                            ProjectName = row.Table.Columns.Contains("ProjectName") ? row.Field<string>("ProjectName") ?? string.Empty : string.Empty,
                            Description = row.Table.Columns.Contains("Description") ? row.Field<string>("Description") ?? string.Empty : string.Empty,
                            ExpenseValue = row.Table.Columns.Contains("ExpenseValue") ? row.Field<decimal?>("ExpenseValue") ?? 0 : 0,
                            ModeOfPay = row.Table.Columns.Contains("ModeOfPay") ? row.Field<string>("ModeOfPay") ?? string.Empty : string.Empty,
                            SpentBy = row.Table.Columns.Contains("SpentBy") ? row.Field<string>("SpentBy") ?? string.Empty : string.Empty
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Optional: log the full exception stack trace if you have a logger
                throw new ApplicationException("Error while loading expense history.", ex);
            }

            return list;
        }

        public List<ExpenseModel> GetExpensesByDateRange(DateTime startDate, DateTime endDate)
        {
            const string sql = @"
        SELECT Expense_Date AS ExpenseOn,
               Other_Expense AS ProjectName,
               Description,
               ModeOf_Pay AS ModeOfPay,
               Expense_Value AS ExpenseValue,
               Spent_By AS SpentBy
        FROM ExpenseManager
        WHERE Expense_Date BETWEEN @StartDate AND @EndDate
        ORDER BY Expense_Date DESC";

            // Use parameters with correct types (not just string) if possible in ExecDataSet
            var parameters = new Dictionary<string, string>
    {
        { "@StartDate", startDate.ToString("yyyy-MM-dd") },
        { "@EndDate", endDate.ToString("yyyy-MM-dd") }
    };

            var ds = _adoHelper.ExecDataSet(sql, parameters) as DataSet;

            var list = new List<ExpenseModel>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    list.Add(new ExpenseModel
                    {
                        ExpenseOn = row.Field<DateTime?>("ExpenseOn") ?? DateTime.MinValue,
                        ProjectName = row.Field<string>("ProjectName") ?? string.Empty,
                        Description = row.Field<string>("Description") ?? string.Empty,
                        ExpenseValue = row.Field<decimal?>("ExpenseValue") ?? 0,
                        ModeOfPay = row.Field<string>("ModeOfPay") ?? string.Empty,
                        SpentBy = row.Field<string>("SpentBy") ?? string.Empty
                    });
                }
            }

            return list;
        }



        //Quotation
        public bool UpdateQuotation(
                             int quoteId,
                             string quoteNumber,
                             DateTime quoteDate,
                             string projectName,
                             decimal grandTotal)
                                {
                                string query = @"
                            UPDATE QuoteMaster SET
                                QuoteNumber = @QuoteNumber,
                                QuoteDate = @QuoteDate,
                                ProjectName = @ProjectName,
                                TotalAmount = @GrandTotal
                            WHERE QuoteID = @QuoteID";

            SqlParameter[] parameters = new SqlParameter[]
            {
            new SqlParameter("@QuoteID", quoteId),
            new SqlParameter("@QuoteNumber", quoteNumber),
            new SqlParameter("@QuoteDate", quoteDate),
            new SqlParameter("@ProjectName", projectName),
            new SqlParameter("@GrandTotal", grandTotal)
            };

            int result = _adoHelper.ExecNonQuery(query);
            return result > 0;
        }  

        public int AddNewQuotation(
                   int customerId,
                   string quoteNumber,
                   DateTime quoteDate,
                   DateTime? expiryDate,
                   string projectName,
                   string subject,
                   decimal discountPercent,
                   string taxType,
                   decimal grandTotal,
                   string customerNotes,
                   string termsAndConditions)

        {
            string query = @"
                        INSERT INTO QuoteMaster 
                        ( CustomerID, QuoteNumber,  QuoteDate, ExpiryDate,  ProjectName, Subject, 
                          DiscountPercent, TaxType, TotalAmount, CustomerNotes, TermsAndConditions)
                        OUTPUT INSERTED.QuoteID
                        VALUES 
                        ( @CustomerID, @QuoteNumber,  @QuoteDate, @ExpiryDate, @ProjectName, @Subject, 
                         @DiscountPercent, @TaxType, @GrandTotal, @CustomerNotes, @TermsAndConditions)";

            var parms = new Dictionary<string, string>
    {
                    //{ "@CustomerID", customerId },
                      { "@CustomerID", customerId.ToString() },
                    { "@QuoteNumber", quoteNumber },
                    //{ "@ReferenceNumber", referenceNumber },
                    { "@QuoteDate", quoteDate.ToString("yyyy-MM-dd") },
                    { "@ExpiryDate", expiryDate?.ToString("yyyy-MM-dd") ?? DBNull.Value.ToString() },
                    //{ "@Salesperson", salesperson },
                    { "@ProjectName", projectName },
                    { "@Subject", subject },
                    //{ "@SubTotal", subTotal.ToString() },
                    { "@DiscountPercent", discountPercent.ToString() },
                    //{ "@DiscountAmount", discountAmount.ToString() },
                    { "@TaxType", taxType },
                    //{ "@TaxName", taxName },
                    //{ "@TaxAmount", taxAmount.ToString() },
                    //{ "@AdjustmentAmount", adjustmentAmount.ToString() },
                    { "@GrandTotal", grandTotal.ToString() },
                    { "@CustomerNotes", customerNotes },
                    { "@TermsAndConditions", termsAndConditions }
    };

            object result = _adoHelper.ExecScalar(query, parms);

            if (result != null && int.TryParse(result.ToString(), out int quotationId))
            {
                return quotationId;
            }

            return -1; // Indicates failure
        }

        public bool AddQuotationItem(int quotationId,

                                      string description,
                                      int quantity,
                                      decimal rate,
                                      decimal tax,
                                      decimal hsn,
                                      string uom
    )
        {
            string query = @"
                    INSERT INTO QuoteItems (QuoteID,   ItemDescription, Quantity, Rate, Tax, HSNCode, UOM)
                    VALUES (@QuoteID, @Description, @Quantity, @Rate, @Tax, @HSN, @UOM )";

            var parms = new Dictionary<string, string>
    {
                        { "@QuoteID", quotationId.ToString() },

                        { "@Description", description },
                        { "@Quantity", quantity.ToString() },
                        { "@Rate", rate.ToString() },
                        { "@Tax", tax.ToString() },
                        { "@HSN", hsn.ToString() },
                        { "@UOM", uom },
                        //{ "@Amount", amount.ToString() }
    };
            try
            {
                return _adoHelper.ExecNonQuery(query, parms) > 0;
            }
            catch (Exception ex)
            {
                // Log the error if needed
                Console.WriteLine("DB Error: " + ex.Message);
                return false;
            }
        }

        public GenerateQuotationModel GetQuotationById(int quoteId)
        {
            string query = @"
        SELECT q.*, 
               c.First_Name, c.Last_Name,
               c.Bn_address1, c.Bn_address2, c.Bn_city, c.Bn_State, c.Bn_pincode, c.Bn_Phone,
               c.sh_address1, c.sh_address2, c.sh_city, c.sh_State, c.sh_pincode, c.sh_Phone
        FROM QuoteMaster q
        JOIN Customers c ON q.CustomerID = c.CustomerID
        WHERE q.QuoteID = @QuoteID";

            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@QuoteID", quoteId)
    };

            var ds = _adoHelper.ExecDataSet(query, parameters);
            if (ds.Tables[0].Rows.Count == 0) return null;

            var row = ds.Tables[0].Rows[0];

            var fullName = $"{row["First_Name"]} {row["Last_Name"]}";

            string billingAddress = string.Join("\n", new[]
            {
        row["Bn_address1"]?.ToString(),
        row["Bn_address2"]?.ToString(),
        row["Bn_city"]?.ToString(),
        row["Bn_State"]?.ToString(),
        row["Bn_pincode"]?.ToString(),
        row["Bn_Phone"]?.ToString()
    }.Where(s => !string.IsNullOrWhiteSpace(s)));

            string shippingAddress = string.Join("\n", new[]
            {
        row["sh_address1"]?.ToString(),
        row["sh_address2"]?.ToString(),
        row["sh_city"]?.ToString(),
        row["sh_State"]?.ToString(),
        row["sh_pincode"]?.ToString(),
        row["sh_Phone"]?.ToString()
    }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var quote = new GenerateQuotationModel
            {
                QuoteID = Convert.ToInt32(row["QuoteID"]),
                CustomerID = Convert.ToString(row["CustomerID"]),
                QuoteNumber = row["QuoteNumber"].ToString(),
                ReferenceNumber = row["ReferenceNumber"].ToString(),
                QuoteDate = row["QuoteDate"] != DBNull.Value ? Convert.ToDateTime(row["QuoteDate"]) : DateTime.MinValue,
                ValidityDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]) : DateTime.MinValue,
                SalesContactName = row["Salesperson"].ToString(),
                ProjectName = row["ProjectName"].ToString(),
                Description = row["Subject"].ToString(),
                SubTotal = row["SubTotal"] as decimal?,
                DiscountPercent = row["DiscountPercent"] as decimal?,
                DiscountAmount = row["DiscountAmount"] as decimal?,
                TaxType = row["TaxType"].ToString(),
                TaxName = row["TaxName"].ToString(),
                TaxAmount = row["TaxAmount"] as decimal?,
                AdjustmentAmount = row["AdjustmentAmount"] as decimal?,
                TotalAmount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0m,
                CustomerName = fullName,
                BillToAddress = billingAddress,
                ShipToAddress = shippingAddress,
                Notes = row["CustomerNotes"].ToString(),
                PaymentTerms = row["TermsAndConditions"].ToString(),
                CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.MinValue,
                Items = GetQuotationItems(quoteId)
            };

            return quote;
        }


        public List<QuotationItemModel> GetQuotationItems(int quoteId)
        {
            string query = "SELECT * FROM QuoteItems WHERE QuoteID = @QuoteID";
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@QuoteID", quoteId)
    };

            var ds = _adoHelper.ExecDataSet(query, parameters);
            var items = new List<QuotationItemModel>();

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                items.Add(new QuotationItemModel
                {
                    Description = row["ItemDescription"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Rate = Convert.ToDecimal(row["Rate"]),
                    Tax = Convert.ToDecimal(row["Tax"]),
                    UnitPrice = Convert.ToDecimal(row["Rate"]),
                    Hsn = Convert.ToInt32(row["HSNCode"]),
                    Uom = row["UOM"].ToString(),
                    Amount = Convert.ToDecimal(row["Amount"])
                });
            }

            return items;
        }



        public List<GenerateQuotationModel> GetAllQuotations()
        {
            string query = "SELECT QuoteID, QuoteNumber, CustomerID FROM QuoteMaster";
            var ds = _adoHelper?.ExecDataSet(query, null);  // Ensure _adoHelper is not null

            var list = new List<GenerateQuotationModel>();

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return list; // Return empty if no data or error

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                list.Add(new GenerateQuotationModel
                {
                    QuoteID = Convert.ToInt32(row["QuoteID"]),
                    QuoteNumber = row["QuoteNumber"].ToString(),
                    CustomerID = row["CustomerID"].ToString()
                });
            }

            return list;
        }

        public string GenerateNextQuoteNumber()
        {
            string query = "SELECT TOP 1 QuoteNumber FROM QuoteMaster ORDER BY QuoteID DESC";
            var ds = _adoHelper.ExecDataSet(query, null);

            string lastQuoteNumber = "Quote-000"; // Default if no records
            int lastNumber = 0;

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                lastQuoteNumber = ds.Tables[0].Rows[0]["QuoteNumber"].ToString();

                // Correct: Split and parse
                string[] parts = lastQuoteNumber.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int parsedNumber))
                {
                    lastNumber = parsedNumber;
                }
            }

            int nextNumber = lastNumber + 1;
            string nextQuoteNumber = $"Quote-{nextNumber:D4}"; // e.g. Quote-0001, Quote-0002

            return nextQuoteNumber;
        }

        public List<QuotationItemModel> GetQuotationItemsByQuoteId(int quoteId)
        {
            string query = "SELECT ItemDescription, HSNCode, UOM, Quantity, Rate, Amount FROM QuoteItems WHERE QuoteID = @QuoteID";

            var parameters = new Dictionary<string, string>
    {
        { "@QuoteID", quoteId.ToString() }
    };

            var ds = _adoHelper.ExecDataSet(query, parameters);
            var items = new List<QuotationItemModel>();

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                items.Add(new QuotationItemModel
                {
                    Description = row["ItemDescription"].ToString(),
                    Hsn = Convert.ToInt32(row["HSNCode"]),
                    Uom = row["UOM"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Rate = Convert.ToDecimal(row["Rate"]),
                    Amount = Convert.ToDecimal(row["Amount"])
                });
            }

            return items;
        }

        public bool SetQuotationInactive(int quoteId)
        {
            string query = "UPDATE QuoteMaster SET IsActive = 0 WHERE QuoteID = @QuoteID";

            var parameters = new Dictionary<string, string>
    {
        { "@QuoteID", quoteId.ToString() }
    };

            int rows = _adoHelper.ExecNonQuery(query, parameters);
            return rows > 0;
        }

        public DataSet GetQuoteMasterData()
        {
            string query = @"
        SELECT 
            QuoteID, QuoteNumber, QuoteDate, ExpiryDate, 
             ProjectName, DiscountPercent, 
            TaxType,  TotalAmount, 
            CustomerNotes, TermsAndConditions
        FROM QuoteMaster";

            return _adoHelper.ExecDataSet(query, null); // Your existing helper that returns a DataSet

        }

        //Invoice
        public int AddNewInvoice(
    int customerId,                          
    string invoiceNumber,
    DateTime invoiceDate,
    DateTime? dueDate,
    string projectName,
    string subject,
    decimal discountPercent,
    string taxType,
    decimal grandTotal,
    string customerNotes,
    string termsAndConditions)
        {
            string query = @"
INSERT INTO InvoiceMaster 
(CustomerID, INVNumber, INVDate, ExpiryDate, ProjectName, Subject, 
 DiscountPercent, TaxType, TotalAmount, CustomerNotes, TermsAndConditions)
OUTPUT INSERTED.InvoiceID
VALUES 
(@CustomerID, @INVNumber, @INVDate, @ExpiryDate, @ProjectName, @Subject, 
 @DiscountPercent, @TaxType, @TotalAmount, @CustomerNotes, @TermsAndConditions);
";

            var parms = new Dictionary<string, string>
    {
        { "@CustomerID", customerId.ToString() },               // 👈 Add this
        { "@INVNumber", invoiceNumber },
        { "@INVDate", invoiceDate.ToString("yyyy-MM-dd") },
        { "@ExpiryDate", dueDate?.ToString("yyyy-MM-dd") ?? DBNull.Value.ToString() },
        { "@ProjectName", projectName },
        { "@Subject", subject },
        { "@DiscountPercent", discountPercent.ToString() },
        { "@TaxType", taxType },
        { "@TotalAmount", grandTotal.ToString() },
        { "@CustomerNotes", customerNotes },
        { "@TermsAndConditions", termsAndConditions }
    };

            object result = _adoHelper.ExecScalar(query, parms);

            if (result != null && int.TryParse(result.ToString(), out int invoiceId))
            {
                return invoiceId;
            }

            return -1; // Indicates failure
        }


        public bool AddInvoiceItem(
    int invoiceId,
    string description,
    int quantity,
    decimal rate,
    decimal tax,
    int hsn,
    string uom)
        {
            string query = @"
    INSERT INTO InvoiceItems
    (InvoiceID, ItemDescription, Quantity, Rate, TaxRate , HSN, UOM)
    VALUES
    (@InvoiceID, @Description, @Quantity, @Rate, @TaxPercent, @HSN, @UOM);";

            var parameters = new Dictionary<string, string>
    {
        { "@InvoiceID", invoiceId.ToString() },
        { "@Description", description },
        { "@Quantity", quantity.ToString() },
        { "@Rate", rate.ToString() },
        { "@TaxPercent", tax.ToString() },
        { "@HSN", hsn.ToString() },
        { "@UOM", uom }
    };

            int rowsAffected = _adoHelper.ExecNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool UpdateInvoice(
    int invoiceId,
    int customerId,
    string invoiceNumber,
    DateTime invoiceDate,
    DateTime? dueDate,
    string projectName,
    string subject,
    decimal discountPercent,
    string taxType,
    decimal grandTotal,
    string customerNotes,
    string termsAndConditions)
        {
            string query = @"
    UPDATE InvoiceMaster SET
        CustomerID = @CustomerID,
        INVNumber = @INVNumber,
        INVDate = @INVDate,
        ExpiryDate = @ExpiryDate,
        ProjectName = @ProjectName,
        Subject = @Subject,
        DiscountPercent = @DiscountPercent,
        TaxType = @TaxType,
        TotalAmount = @TotalAmount,
        CustomerNotes = @CustomerNotes,
        TermsAndConditions = @TermsAndConditions
    WHERE InvoiceID = @InvoiceID;";

            var parms = new Dictionary<string, string>
    {
        { "@InvoiceID", invoiceId.ToString() },
        { "@CustomerID", customerId.ToString() },
        { "@INVNumber", invoiceNumber },
        { "@INVDate", invoiceDate.ToString("yyyy-MM-dd") },
        { "@ExpiryDate", dueDate?.ToString("yyyy-MM-dd") ?? DBNull.Value.ToString() },
        { "@ProjectName", projectName },
        { "@Subject", subject },
        { "@DiscountPercent", discountPercent.ToString() },
        { "@TaxType", taxType },
        { "@TotalAmount", grandTotal.ToString() },
        { "@CustomerNotes", customerNotes },
        { "@TermsAndConditions", termsAndConditions }
    };

            int result = _adoHelper.ExecNonQuery(query, parms);
            return result > 0;
        }

        public bool DeleteInvoiceItems(int invoiceId)
        {
            string query = "DELETE FROM InvoiceItems WHERE InvoiceID = @InvoiceID";
            var parms = new Dictionary<string, string>
    {
        { "@InvoiceID", invoiceId.ToString() }
    };

            int result = _adoHelper.ExecNonQuery(query, parms);
            return result >= 0; // deletion may return 0 if no rows, that's okay
        } 
        public string GenerateNextInvoiceNumber()
        {
            string query = "SELECT TOP 1 INVNumber FROM napeans.InvoiceMaster ORDER BY InvoiceID DESC";
            var ds = _adoHelper.ExecDataSet(query, null);

            string lastInvoiceNumber = "Invoice-0000"; // Default if no records
            int lastNumber = 0;

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                lastInvoiceNumber = ds.Tables[0].Rows[0]["INVNumber"].ToString();

                // Split and parse the number part
                string[] parts = lastInvoiceNumber.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int parsedNumber))
                {
                    lastNumber = parsedNumber;
                }
            }

            int nextNumber = lastNumber + 1;
            string nextInvoiceNumber = $"Invoice-{nextNumber:D4}"; // e.g. Invoice-0001, Invoice-0002

            return nextInvoiceNumber;
        }

        public DataSet GetInvoiceMasterData()
        {
            string query = @"
        SELECT  
    im.InvoiceID,
    im.INVNumber,
    im.INVDate,
    im.ExpiryDate,
    im.CustomerID,
    im.ProjectID,
    im.Subject,
    im.CustomerNotes,
    im.TermsAndConditions,
    im.CreatedDate,
    im.ModifiedDate,
    im.CreatedBy,
    im.ModifiedBy,
    im.TaxType,
    im.DiscountPercent,
    im.TotalAmount,
    im.AmountInWords,
    im.ProjectName,
    im.INVStatus,
    ISNULL(SUM(ip.AmountReceived), 0) AS AmountReceived
FROM InvoiceMaster im
LEFT JOIN broker.InvoicePayments ip ON im.InvoiceID = ip.InvoiceID AND ip.IsActive = 1
WHERE im.IsActive = 1
GROUP BY 
    im.InvoiceID,
    im.INVNumber,
    im.INVDate,
    im.ExpiryDate,
    im.CustomerID,
    im.ProjectID,
    im.Subject,
    im.CustomerNotes,
    im.TermsAndConditions,
    im.CreatedDate,
    im.ModifiedDate,
    im.CreatedBy,
    im.ModifiedBy,
    im.TaxType,
    im.DiscountPercent,
    im.TotalAmount,
    im.AmountInWords,
    im.ProjectName,
    im.INVStatus
    ";

            return _adoHelper.ExecDataSet(query, null);
        }

        // AppSettings
        public bool SaveAppSettings(AppSettingsModel settings)
        {
            string query = @"
IF EXISTS (SELECT 1 FROM Broker.AppSettings)
BEGIN
    UPDATE Broker.AppSettings SET
        CompanyName = @CompanyName,
        CompanyAddress = @CompanyAddress,
        GSTNumber = @GSTNumber,
        CompanyEmail = @CompanyEmail,
        CompanyPhone = @CompanyPhone,
        PaymentsDueDays = @PaymentsDueDays,
        SalesPersonName = @SalesPersonName,
        BankName = @BankName,
        AccountNumber = @AccountNumber,
        IFSCCode = @IFSCCode,
        BranchAddress = @BranchAddress,
        CompanyLogoPath = @CompanyLogoPath,
        AuthorizedSignPath = @AuthorizedSignPath,
        LastUpdated = GETDATE()
END
ELSE
BEGIN
    INSERT INTO Broker.AppSettings (
        CompanyName, CompanyAddress, GSTNumber, CompanyEmail, CompanyPhone,
        PaymentsDueDays, SalesPersonName,
        BankName, AccountNumber, IFSCCode, BranchAddress,
        CompanyLogoPath, AuthorizedSignPath
    ) VALUES (
        @CompanyName, @CompanyAddress, @GSTNumber, @CompanyEmail, @CompanyPhone,
        @PaymentsDueDays, @SalesPersonName,
        @BankName, @AccountNumber, @IFSCCode, @BranchAddress,
        @CompanyLogoPath, @AuthorizedSignPath
    )
END";

            var parameters = new Dictionary<string, string>
    {
        { "@CompanyName", settings.CompanyName },
        { "@CompanyAddress", settings.CompanyAddress },
        { "@GSTNumber", settings.GSTNumber },
        { "@CompanyEmail", settings.CompanyEmail },
        { "@CompanyPhone", settings.CompanyPhone },
        { "@PaymentsDueDays", settings.PaymentsDueDays.ToString() },
        { "@SalesPersonName", settings.SalesPersonName },
        { "@BankName", settings.BankName },
        { "@AccountNumber", settings.AccountNumber },
        { "@IFSCCode", settings.IFSCCode },
        { "@BranchAddress", settings.BranchAddress },
        { "@CompanyLogoPath", settings.CompanyLogoPath },
        { "@AuthorizedSignPath", settings.AuthorizedSignPath }
    };

            return _adoHelper.ExecNonQuery(query, parameters) > 0;
        }
        public AppSettingsModel LoadAppSettings()
        {
            string query = "SELECT TOP 1 * FROM Broker.AppSettings ORDER BY SettingID DESC";
            var ds = _adoHelper.ExecDataSet(query, null);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                return new AppSettingsModel
                {
                    CompanyName = row["CompanyName"].ToString(),
                    CompanyAddress = row["CompanyAddress"].ToString(),
                    GSTNumber = row["GSTNumber"].ToString(),
                    CompanyEmail = row["CompanyEmail"].ToString(),
                    CompanyPhone = row["CompanyPhone"].ToString(),
                    PaymentsDueDays = Convert.ToInt32(row["PaymentsDueDays"]),
                    SalesPersonName = row["SalesPersonName"].ToString(),
                    BankName = row["BankName"].ToString(),
                    AccountNumber = row["AccountNumber"].ToString(),
                    IFSCCode = row["IFSCCode"].ToString(),
                    BranchAddress = row["BranchAddress"].ToString(),
                    CompanyLogoPath = row["CompanyLogoPath"].ToString(),
                    AuthorizedSignPath = row["AuthorizedSignPath"].ToString()
                };
            }

            return null;
        }


        // Delivery Challan
        public bool AddDeliveryChallanItem(
    int deliveryChallanId,
    string description,
    int quantity,
    string uom)
        {
            string query = @"
INSERT INTO Broker.DeliveryChallanItems
(DeliveryChallanId, Description, Quantity, UOM)
VALUES
(@DeliveryChallanId, @Description, @Quantity, @UOM);";

            var parameters = new Dictionary<string, string>
    {
        { "@DeliveryChallanId", deliveryChallanId.ToString() },
        { "@Description", description },
        { "@Quantity", quantity.ToString() },
        { "@UOM", uom }
    };

            int rowsAffected = _adoHelper.ExecNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public int AddNewDeliveryChallan(
    string challanNumber,
    DateTime challanDate,
    string billToAddress,
    string shipToAddress,
    string remarks)
        {
            string query = @"
    INSERT INTO Broker.DeliveryChallanMaster 
(ChallanNumber, ChallanDate, BillToAddress, ShipToAddress, Remarks)
VALUES 
    (@ChallanNumber, @ChallanDate, @BillToAddress, @ShipToAddress, @Remarks);
    SELECT SCOPE_IDENTITY();";

            var parameters = new Dictionary<string, string>
    {
        { "@ChallanNumber", challanNumber },
        { "@ChallanDate", challanDate.ToString("yyyy-MM-dd") },
        { "@BillToAddress", billToAddress },
        { "@ShipToAddress", shipToAddress },
        { "@Remarks", remarks }
    };

            object result = _adoHelper.ExecScalar(query, parameters);

            return result != null ? Convert.ToInt32(result) : -1;
        } 

        public List<ChallanItemModel> GetDeliveryChallanItemsByDCId(int dcId)
        {
            string query = @"
        SELECT Description, Quantity, Uom
        FROM Broker.DeliveryChallanItems
        WHERE DeliveryChallanId = @DeliveryChallanId";

            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@DeliveryChallanId", dcId)
    };

            var ds = _adoHelper.ExecDataSet(query, parameters);
            var items = new List<ChallanItemModel>();

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                items.Add(new ChallanItemModel
                {
                    Description = row["Description"]?.ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Uom = row["Uom"]?.ToString()
                });
            }

            return items;
        }

        public string GenerateNextDeliveryChallanNumber()
        {
            string query = "SELECT TOP 1 ChallanNumber FROM Broker.DeliveryChallanMaster ORDER BY DeliveryChallanId DESC";
            var ds = _adoHelper.ExecDataSet(query, null);

            string lastDCNumber = "DC-0000"; // Default if no records
            int lastNumber = 0;

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                lastDCNumber = ds.Tables[0].Rows[0]["ChallanNumber"].ToString();

                // Split and parse the number part
                string[] parts = lastDCNumber.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int parsedNumber))
                {
                    lastNumber = parsedNumber;
                }
            }

            int nextNumber = lastNumber + 1;
            string nextDCNumber = $"DC-{nextNumber:D4}"; 

            return nextDCNumber;
        }
       

      

        public DataSet GetDeliveryChallanData()
        {
            string query = @"
       SELECT
    DeliveryChallanId, ChallanNumber, ChallanDate,
    BillToAddress, ShipToAddress, Remarks, CreatedAt
FROM Broker.DeliveryChallanMaster
";

            return _adoHelper.ExecDataSet(query, null); // Your existing helper that returns a DataSet
        }


        // Invoice Payments
        public int AddInvoicePayment(
     int invoiceId,
     DateTime paymentDate,
     decimal amountReceived,
     decimal taxDeducted,
     decimal igst,
     decimal cgst)
        {
            string query = @"
    INSERT INTO Broker.InvoicePayments 
        (InvoiceID, PaymentDate, AmountReceived, TaxDeducted, IGST, CGST, CreatedAt, IsActive)
    VALUES 
        (@InvoiceID, @PaymentDate, @AmountReceived, @TaxDeducted, @IGST, @CGST, GETDATE(), 1);
    SELECT SCOPE_IDENTITY();";

            var parameters = new Dictionary<string, string>
    {
        { "@InvoiceID", invoiceId.ToString() },
        { "@PaymentDate", paymentDate.ToString("yyyy-MM-dd") },
        { "@AmountReceived", amountReceived.ToString() },
        { "@TaxDeducted", taxDeducted.ToString() },
        { "@IGST", igst.ToString() },
        { "@CGST", cgst.ToString() }
    };

            object result = _adoHelper.ExecScalar(query, parameters);
            int insertedId = result != null ? Convert.ToInt32(result) : -1;

           
            UpdateInvoiceStatusBasedOnPayments(invoiceId);

            return insertedId;
        }

        private void UpdateInvoiceStatusBasedOnPayments(int invoiceId)
        {
            // 1. Get total paid
            decimal totalPaid = GetTotalPaidForInvoice(invoiceId);

            // 2. Get invoice total
            string query = "SELECT TotalAmount FROM InvoiceMaster WHERE InvoiceID = @InvoiceID";
            var invoiceDs = _adoHelper.ExecDataSet(query, new Dictionary<string, string>
    {
        { "@InvoiceID", invoiceId.ToString() }
    });

            if (invoiceDs.Tables.Count == 0 || invoiceDs.Tables[0].Rows.Count == 0)
                return;

            decimal totalInvoiceAmount = Convert.ToDecimal(invoiceDs.Tables[0].Rows[0]["TotalAmount"]);

            // 3. Decide status
            string status;
            if (totalPaid >= totalInvoiceAmount)
                status = "PAYMENT RECEIVED";
            else if (totalPaid > 0)
                status = "PARTIAL PAYMENT RECEIVED";
            else
                status = "PENDING";

             
            string updateStatusQuery = "UPDATE InvoiceMaster SET INVStatus = @Status WHERE InvoiceID = @InvoiceID";
            var updateParams = new Dictionary<string, string>
    {
        { "@Status", status },
        { "@InvoiceID", invoiceId.ToString() }
    };

            _adoHelper.ExecNonQuery(updateStatusQuery, updateParams);
        } 
        public decimal GetTotalPaidForInvoice(int invoiceId)
        {
            string query = "SELECT ISNULL(SUM(AmountReceived), 0) FROM Broker.InvoicePayments WHERE InvoiceID = @InvoiceID";
            var parameters = new Dictionary<string, string>
                {
                    { "@InvoiceID", invoiceId.ToString() }
                };

            object result = _adoHelper.ExecScalar(query, parameters);

            return Convert.ToDecimal(result);

        } 
        public List<InvoicePaymentModel> GetPaymentsByInvoiceId(int invoiceId)
        {
            string query = "SELECT AmountReceived, PaymentDate FROM Broker.InvoicePayments WHERE InvoiceID = @InvoiceID";
            var parameters = new Dictionary<string, string>
    {
        { "@InvoiceID", invoiceId.ToString() }
    };

            var ds = _adoHelper.ExecDataSet(query, parameters);

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return new List<InvoicePaymentModel>();

            return ds.Tables[0].AsEnumerable().Select(row => new InvoicePaymentModel
            {
                PaidAmount = row.Field<decimal>("AmountReceived"),
                PaymentDate = row.Field<DateTime>("PaymentDate"),
                IsSummaryRow = false
            }).ToList();
        }
        public GenerateInvoiceModel GetInvoiceById(int invoiceId)
        {
            string query = @"
   SELECT i.*, 
           c.First_Name, c.Last_Name,
           c.Bn_address1, c.Bn_address2, c.Bn_city, c.Bn_State, c.Bn_pincode, c.Bn_Phone,
           c.sh_address1, c.sh_address2, c.sh_city, c.sh_State, c.sh_pincode, c.sh_Phone
    FROM napeans.InvoiceMaster i
    JOIN Customers c ON i.CustomerID = c.CustomerID WHERE i.InvoiceID = @InvoiceID";

            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@InvoiceID", invoiceId)
    };

            var ds = _adoHelper.ExecDataSet(query, parameters);
            if (ds.Tables[0].Rows.Count == 0) return null;

            var row = ds.Tables[0].Rows[0];

            var fullName = $"{row["First_Name"]} {row["Last_Name"]}";

            string billingAddress = string.Join("\n", new[]
            {
        row["Bn_address1"]?.ToString(),
        row["Bn_address2"]?.ToString(),
        row["Bn_city"]?.ToString(),
        row["Bn_State"]?.ToString(),
        row["Bn_pincode"]?.ToString(),
        row["Bn_Phone"]?.ToString()
    }.Where(s => !string.IsNullOrWhiteSpace(s)));

            string shippingAddress = string.Join("\n", new[]
            {
        row["sh_address1"]?.ToString(),
        row["sh_address2"]?.ToString(),
        row["sh_city"]?.ToString(),
        row["sh_State"]?.ToString(),
        row["sh_pincode"]?.ToString(),
        row["sh_Phone"]?.ToString()
    }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var invoice = new GenerateInvoiceModel
            {
                InvoiceId = Convert.ToInt32(row["InvoiceID"]),
                CustomerID = Convert.ToString(row["CustomerID"]),
                InvoiceNumber = row["INVNumber"].ToString(),
                //ReferenceNumber = row["ReferenceNumber"]?.ToString(),
                InvoiceDate = row["INVDate"] != DBNull.Value ? Convert.ToDateTime(row["INVDate"]) : DateTime.MinValue,
                DueDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]) : DateTime.MinValue,
                //Salesperson = row["Salesperson"]?.ToString(),
                //ProjectName = row["ProjectName"]?.ToString(),
                Description = row["Subject"]?.ToString(),
                ///SubTotal = row["SubTotal"] as decimal?,
                //DiscountPercent = row["DiscountPercent"] as decimal?,
                //DiscountAmount = row["DiscountAmount"] as decimal?,
                //TaxType = row["TaxType"]?.ToString(),
                //TaxName = row["TaxName"]?.ToString(),
                //TaxAmount = row["TaxAmount"] as decimal?,
                //AdjustmentAmount = row["AdjustmentAmount"] as decimal?,
                //TotalAmount = row["TotalAmount"] as decimal?,
                FullName = fullName,
                BillToAddress = billingAddress,
                ShipToAddress = shippingAddress,
                Notes = row["CustomerNotes"]?.ToString(),
                PaymentTerms = row["TermsAndConditions"]?.ToString(),
                //CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.MinValue,
                Items = GetInvoiceItems(invoiceId) // Implement similar to GetQuotationItems
            };

            return invoice;
        }
        public List<InvoiceItemModel> GetInvoiceItems(int invoiceId)
        {
            string query = "SELECT ItemDescription, Quantity ,Rate ,TaxRate ,Amount, HSN ,UOM, TotalAmount , TaxAmount  FROM napeans.InvoiceItems WHERE InvoiceID = @InvoiceID";

            var parameters = new Dictionary<string, string>
    {
        { "@InvoiceID", invoiceId.ToString() }
    };

            var ds = _adoHelper.ExecDataSet(query, parameters);
            var items = new List<InvoiceItemModel>();

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                items.Add(new InvoiceItemModel
                {
                    Description = row["ItemDescription"].ToString(),
                    Hsn = Convert.ToInt32(row["HSN"]),
                    Uom = row["UOM"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Rate = Convert.ToDecimal(row["Rate"]),
                    Tax = Convert.ToDecimal(row["TaxRate"]),
                    //DiscountPercent = Convert.ToDecimal(row["DiscountPercent"]),
                    Amount = Convert.ToDecimal(row["Amount"])
                });
            }

            return items;
        }
        public bool SoftDeleteInvoice(int invoiceId)
        {
            string query = "UPDATE napeans.InvoiceMaster SET IsActive = 0 WHERE InvoiceID = @InvoiceID";

            var parameters = new Dictionary<string, string>
    {
        { "@InvoiceID", invoiceId.ToString() }
    };

            int rowsAffected = _adoHelper.ExecNonQuery(query, parameters);
            return rowsAffected > 0;
        } 
        public bool UpdateInvoiceStatus(int invoiceId, string newStatus)
        {
            string query = "UPDATE napeans.InvoiceMaster SET INVStatus = @INVStatus WHERE InvoiceID = @InvoiceID";

            var parameters = new Dictionary<string, string>
    {
        { "@INVStatus", newStatus },
        { "@InvoiceID", invoiceId.ToString() }
    };

            int rowsAffected = _adoHelper.ExecNonQuery(query, parameters);

            return rowsAffected > 0;
        }

        public FullQuotationModel GetFullQuotationById(int quoteId)
        {
            string masterQuery = @"SELECT qm.*, c.first_name, c.last_name
                           FROM quotemaster qm
                           JOIN customers c ON qm.customerid = c.customerid
                           WHERE qm.quoteid = @quoteid";

            string itemQuery = "SELECT * FROM quoteitems WHERE quoteid = @quoteid";

            var parameters = new Dictionary<string, string> { { "@quoteid", quoteId.ToString() } };

            var masterTable = _adoHelper.ExecDataSet(masterQuery, parameters)?.Tables[0];
            var itemTable = _adoHelper.ExecDataSet(itemQuery, parameters)?.Tables[0];

            if (masterTable?.Rows.Count > 0)
            {
                var row = masterTable.Rows[0];
                var fullName = $"{row["First_Name"]} {row["Last_Name"]}";

                var master = new GenerateQuotationModel
                {
                    QuoteID = Convert.ToInt32(row["QuoteId"]),
                    QuoteNumber = row["QuoteNumber"]?.ToString(),
                    QuoteDate = Convert.ToDateTime(row["QuoteDate"]),
                    CustomerID = row["CustomerId"]?.ToString(),
                    ProjectName = row["ProjectName"]?.ToString(),
                    FullName = fullName,
                    Description = row["Subject"]?.ToString(),
                    PaymentTerms = row["TermsAndConditions"]?.ToString(),
                    Notes = row["CustomerNotes"]?.ToString(),
                    TotalAmount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0,
                    Items = new List<QuotationItemModel>()
                };

                var items = itemTable?.AsEnumerable().Select(itemRow => new QuotationItemModel
                {
                    Description = itemRow["Itemdescription"]?.ToString(),
                    Hsn = itemRow["HSNCode"] != DBNull.Value ? Convert.ToInt32(itemRow["HSNCode"]) : 0,
                    Uom = itemRow["UOM"]?.ToString(),
                    Quantity = itemRow["Quantity"] != DBNull.Value ? Convert.ToInt32(itemRow["Quantity"]) : 0,
                    Rate = itemRow["Rate"] != DBNull.Value ? Convert.ToDecimal(itemRow["Rate"]) : 0,
                    Tax = itemRow["Tax"] != DBNull.Value ? Convert.ToDecimal(itemRow["Tax"]) : 0,
                    Amount = itemRow["Amount"] != DBNull.Value ? Convert.ToDecimal(itemRow["Amount"]) : 0
                }).ToList() ?? new List<QuotationItemModel>();

                master.Items = items;

                return new FullQuotationModel
                {
                    Master = master,
                    Items = items
                };
            }

            return null;
        }


        public FullInvoiceModel GetFullInvoiceById(int invoiceId)
        {
            string masterQuery = "SELECT im.*, c.First_Name, c.Last_Name\r\nFROM InvoiceMaster im\r\nJOIN Customers c ON im.CustomerID = c.CustomerID\r\nWHERE im.InvoiceID = @InvoiceID\r\n";
            string itemQuery = "SELECT * FROM InvoiceItems WHERE InvoiceID = @InvoiceID";

            var parameters = new Dictionary<string, string> { { "@InvoiceID", invoiceId.ToString() } };

            var masterTable = _adoHelper.ExecDataSet(masterQuery, parameters)?.Tables[0];
            var itemTable = _adoHelper.ExecDataSet(itemQuery, parameters)?.Tables[0];

            if (masterTable?.Rows.Count > 0)
            {
                var row = masterTable.Rows[0];
                var fullName = $"{row["First_Name"]} {row["Last_Name"]}";

                var master = new GenerateInvoiceModel
                {
                    InvoiceId = Convert.ToInt32(row["InvoiceID"]),
                    InvoiceNumber = row["INVNumber"]?.ToString(),
                    InvoiceDate = Convert.ToDateTime(row["INVDate"]),
                    DueDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]) : DateTime.MinValue,
                    CustomerID = row["CustomerID"]?.ToString(),
                    ProjectName = row["ProjectName"]?.ToString(),
                    FullName = fullName, 
                    Description = row["Subject"]?.ToString(), 
                    PaymentTerms = row["TermsAndConditions"]?.ToString(),
                    Notes = row["CustomerNotes"]?.ToString(), 
                    Items = new List<InvoiceItemModel>() // Will populate below
                };

                var items = itemTable?.AsEnumerable().Select(itemRow => new InvoiceItemModel
                {
                    InvoiceId = Convert.ToInt32(itemRow["InvoiceID"]),
                    Description = itemRow["ItemDescription"]?.ToString(),
                    Hsn = itemRow["HSN"] != DBNull.Value ? Convert.ToInt32(itemRow["HSN"]) : 0,
                    Uom = itemRow["UOM"]?.ToString(),
                    Quantity = itemRow["Quantity"] != DBNull.Value ? Convert.ToInt32(itemRow["Quantity"]) : 0,
                    //UnitPrice = itemRow["Rate"] != DBNull.Value ? Convert.ToDecimal(itemRow["Rate"]) : 0,
                    Rate = itemRow["Rate"] != DBNull.Value ? Convert.ToDecimal(itemRow["Rate"]) : 0,
                    Tax = itemRow["TaxRate"] != DBNull.Value ? Convert.ToDecimal(itemRow["TaxRate"]) : 0,
                    Amount = itemRow["Amount"] != DBNull.Value ? Convert.ToDecimal(itemRow["Amount"]) : 0,
                    SubItems = null // Optional
                }).ToList() ?? new List<InvoiceItemModel>();

                master.Items = items;

                return new FullInvoiceModel
                {
                    Master = master,
                    Items = items
                };
            }

            return null;
        }

    }
}

