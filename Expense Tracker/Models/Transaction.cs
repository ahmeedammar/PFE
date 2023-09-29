using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Expense_Tracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        public int Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return Category == null ? "" : Category.Icon + " " + Category.Title;
            }
        }

        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-TN");
                return ((Category == null || Category.Type == "Expense") ? "- " : "+ ") + Amount.ToString("C", culture);
            }
        }

        public string? Currency { get; set; } // Add the Currency property

        [NotMapped]
        public decimal ConvertedAmount { get; set; } // Use decimal for converted amount

        // Method to transform ConvertedAmount to int for further calculations
        public int GetConvertedAmountAsInt()
        {
            return (int)ConvertedAmount;
        }
        [Column(TypeName = "nvarchar(100)")]
        public string? Supplier { get; set; }
    }

}
