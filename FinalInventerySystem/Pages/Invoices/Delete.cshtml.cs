using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FinalInventerySystem.Pages.Invoices
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDBcontext _context;
        public DeleteModel(ApplicationDBcontext context) { _context = context; }

        [BindProperty]
        public Invoice? Invoice { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Inventory)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (Invoice == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Invoice deleted successfully!";
            return RedirectToPage("Index");
        }
    }
}