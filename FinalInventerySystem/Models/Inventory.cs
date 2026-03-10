using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FinalInventerySystem.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = "";

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = "";

        public string? Description { get; set; }

        // base price stored with precision
        [Precision(16, 2)]
        public decimal BasePrice { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
