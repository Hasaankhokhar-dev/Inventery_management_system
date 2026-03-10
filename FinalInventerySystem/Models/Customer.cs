using System.ComponentModel.DataAnnotations;

namespace FinalInventerySystem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "Phone number must be exactly 11 digits")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits")]
        public string Phone { get; set; }
        // adress
        [Required(ErrorMessage = "Adress is required")]
        public string Address { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}