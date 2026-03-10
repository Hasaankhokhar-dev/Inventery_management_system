using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FinalInventerySystem.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [ValidateNever]   // 🚀 prevent validation error on POST
        public Invoice? Invoice { get; set; }   // allow null, EF will link via InvoiceId

        [Required]
        public int InventoryId { get; set; }

        [ValidateNever]   // 🚀 prevent validation error on POST
        public Inventory? Inventory { get; set; }   // allow null, EF will link via InventoryId

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }
    }
}
