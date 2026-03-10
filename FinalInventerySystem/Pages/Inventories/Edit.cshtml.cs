using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FinalInventerySystem.Pages.Inventories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDBcontext _context;

        public EditModel(ApplicationDBcontext context)
        {
            _context = context;
        }

        [BindProperty]
        public Inventory Inventory { get; set; } = new Inventory();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Inventory = await _context.Inventories.FindAsync(id);

            if (Inventory == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Inventory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Inventories.Any(e => e.Id == Inventory.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("Index");
        }
    }
}
