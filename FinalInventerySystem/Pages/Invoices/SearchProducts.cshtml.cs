using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace FinalInventerySystem.Pages.Invoices
{
    // AJAX search for products in Inventory
    public class SearchProductsModel : PageModel
    {
        private readonly ApplicationDBcontext _context;

        public SearchProductsModel(ApplicationDBcontext context)
        {
            _context = context;
        }

        public JsonResult OnGet(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new JsonResult(new { });

            var results = _context.Inventories
                .Where(p => p.Name.Contains(query))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    basePrice = p.BasePrice,
                    quantity = p.Quantity
                })
                .Take(10)
                .ToList();

            return new JsonResult(results);
        }
    }
}
