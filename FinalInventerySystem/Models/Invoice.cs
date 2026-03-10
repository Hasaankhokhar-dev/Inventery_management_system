using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalInventerySystem.Models
{
    public class Invoice
{
    public int Id { get; set; }

    [Required]
    public string CustomerName { get; set; } = "";

    [Required]
    public string Phone { get; set; } = "";

    [Required]
    public string Address { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    // ✅ NEW: Invoice creation date (Edit nahi hogi)
     public DateTime CreatedAt { get; set; } = DateTime.Now;

    public decimal AdjustmentAmount { get; set; }

    public string AdjustmentType { get; set; } = "+";

    public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
}