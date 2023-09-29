using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Project name is required.")]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Hours of work is required.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HoursOfWork { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Total { get; set; }

        public virtual Client Client { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
