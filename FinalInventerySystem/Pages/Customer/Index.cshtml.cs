using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinalInventerySystem.Models;  // ? Customer model yahan hai
using FinalInventerySystem.Services;

namespace FinalInventerySystem.Pages.Customer  // ? Yeh alag namespace hai
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBcontext _context;

        public IndexModel(ApplicationDBcontext context)
        {
            _context = context;
        }

        // ? Explicitly specify Models namespace
        public List<Models.Customer> Customers { get; set; } = new List<Models.Customer>();

        [BindProperty]
        public Models.Customer NewCustomer { get; set; }  // ? Models.Customer use karein

        public async Task OnGetAsync()
        {
            await LoadCustomersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCustomersAsync();
                return Page();
            }

            _context.Customers.Add(NewCustomer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Electrician added successfully!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Electrician deleted successfully!";
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadCustomersAsync()
        {
            Customers = await _context.Customers
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }
    }
}




