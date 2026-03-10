
using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace FinalInventerySystem.Pages.Inventories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDBcontext _context;

        public CreateModel(ApplicationDBcontext context)
        {
            _context = context;
        }

        [BindProperty]
        public Inventory Inventory { get; set; } = new Inventory();

        // ? YEH PROPERTY MISSING THI - ADD KARO
        [BindProperty(SupportsGet = true)]
        public int ReturnPage { get; set; } = 1;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Inventories.Add(Inventory);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { pageIndex = ReturnPage });
        }
    }
}