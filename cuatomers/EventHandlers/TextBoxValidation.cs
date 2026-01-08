using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace cuatomers.EventHandlers
{
    internal class TextBoxValidation
    {
        public static bool ValidateEmail(TextBox txtBox, TextBlock errorText)
        {
            string email = txtBox.Text.Trim();
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (string.IsNullOrEmpty(email))
            {
                txtBox.ClearValue(Control.BorderBrushProperty);
                errorText.Text = "";
                return false;
            }
            else if (Regex.IsMatch(email, pattern))
            {
                txtBox.BorderBrush = Brushes.Green;
                errorText.Text = "";
                return true;
            }
            else
            {
                txtBox.BorderBrush = Brushes.Red;
                errorText.Text = "Invalid email format!";
                return false;
            }
        }

        public static bool ValidateMobile(TextBox txtBox, TextBlock errorText)
        {
            string mobile = txtBox.Text.Trim();
            string pattern = @"^\d{10}$";

            if (string.IsNullOrEmpty(mobile))
            {
                txtBox.ClearValue(Control.BorderBrushProperty);
                errorText.Text = "";
                return false;
            }
            else if (Regex.IsMatch(mobile, pattern))
            {
                txtBox.BorderBrush = Brushes.Green;
                errorText.Text = "";
                return true;
            }
            else
            {
                txtBox.BorderBrush = Brushes.Red;
                errorText.Text = "Invalid mobile number!";
                return false;
            }
        }
        public static bool ValidateName(TextBox txtBox, TextBlock errorText)
        {
            string name = txtBox.Text.Trim();
            string pattern = @"^[A-Za-z\s]{2,}$"; // Letters and spaces, at least 2 chars

            if (string.IsNullOrEmpty(name))
            {
                txtBox.ClearValue(Control.BorderBrushProperty);
                errorText.Text = "";
                return false;
            }
            else if (Regex.IsMatch(name, pattern))
            {
                txtBox.BorderBrush = Brushes.Green;
                errorText.Text = "";
                return true;
            }
            else
            {
                txtBox.BorderBrush = Brushes.Red;
                errorText.Text = "Invalid name! Only letters allowed.";
                return false;
            }
        }

        public static bool ValidateGST(TextBox txtBox, TextBlock errorText)
        {
            string email = txtBox.Text.Trim();
            string pattern = @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$";

            if (string.IsNullOrEmpty(email))
            {
                txtBox.ClearValue(Control.BorderBrushProperty);
                errorText.Text = "";
                return false;
            }
            else if (Regex.IsMatch(email, pattern))
            {
                txtBox.BorderBrush = Brushes.Green;
                errorText.Text = "";
                return true;
            }
            else
            {
                txtBox.BorderBrush = Brushes.Red;
                errorText.Text = "Invalid GST Number format!";
                return false;
            }
        }
        public static bool IsValidPincode(TextBox txtBox, TextBlock errorText)
        {
            string pincode = txtBox.Text.Trim();
            string pattern = @"^[1-9][0-9]{5}$";

            if (string.IsNullOrEmpty(pincode))
            {
                txtBox.ClearValue(Control.BorderBrushProperty);
                errorText.Text = "";
                return false;
            }
            else if (Regex.IsMatch(pincode, pattern))
            {
                txtBox.BorderBrush = Brushes.Green;
                errorText.Text = "";
                return true;
            }
            else
            {
                txtBox.BorderBrush = Brushes.Red;
                errorText.Text = "Invalid PIN Code format!";
                return false;
            }
        }
        public static bool IsValidvalue(TextBox txtBox, TextBlock errorText)
        {
            string amount = txtBox.Text.Trim();
            string pattern = @"[\d.]";

            if (string.IsNullOrEmpty(amount))
            {
                txtBox.ClearValue(Control.BorderBrushProperty);
                errorText.Text = "";
                return false;
            }
            else if (Regex.IsMatch(amount, pattern))
            {
                txtBox.BorderBrush = Brushes.Green;
                errorText.Text = "";
                return true;
            }
            else
            {
                txtBox.BorderBrush = Brushes.Red;
                errorText.Text = "Invalid Value format!";
                return false;
            }
        }
    }
}
