using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }
    }
}
