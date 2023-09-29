namespace Expense_Tracker.Models
{
    public class InvoiceViewModel
    {
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string EmployeeName { get; set; }
        public decimal HoursOfWork { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal Total { get; set; }
    }

}
