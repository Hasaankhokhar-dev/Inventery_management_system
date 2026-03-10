using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalInventerySystem.Pages.Inventories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDBcontext _context;

        public DeleteModel(ApplicationDBcontext context)
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
            var inventory = await _context.Inventories.FindAsync(Inventory.Id);

            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }
}
