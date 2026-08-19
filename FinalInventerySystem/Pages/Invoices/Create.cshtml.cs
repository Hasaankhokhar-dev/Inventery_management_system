using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalInventerySystem.Pages.Invoices
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDBcontext _context;

        public CreateModel(ApplicationDBcontext context)
        {
            _context = context;
        }

        [BindProperty]
        public Invoice Invoice { get; set; } = new Invoice();

        [BindProperty]
        public List<InvoiceItemInput> SelectedItems { get; set; } = new();

        public List<Inventory> AvailableInventories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int ReturnPage { get; set; } = 1;

        public void OnGet(int? returnPage)
        {
            AvailableInventories = _context.Inventories.ToList();
            if (returnPage.HasValue) ReturnPage = returnPage.Value;
        }

        // ✅ Quick product add handler
        public async Task<IActionResult> OnPostAddQuickProductAsync(
            [FromBody] QuickProductInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Name) || input.BasePrice <= 0 || input.Quantity < 0)
            {
                return new JsonResult(new { success = false, message = "Invalid product data." });
            }

            var existing = _context.Inventories
                .FirstOrDefault(x => x.Name.ToLower() == input.Name.ToLower());

            if (existing != null)
            {
                existing.Quantity += input.Quantity;
                if (input.BasePrice > 0)
                    existing.BasePrice = input.BasePrice;

                await _context.SaveChangesAsync();

                return new JsonResult(new
                {
                    success = true,
                    isExisting = true,
                    id = existing.Id,
                    name = existing.Name,
                    basePrice = existing.BasePrice,
                    quantity = existing.Quantity,
                    message = $"Product already exists! Stock updated to {existing.Quantity}."
                });
            }

            var newProduct = new Inventory
            {
                Code = "QK-" + DateTime.Now.Ticks.ToString().Substring(10),
                Name = input.Name.Trim(),
                BasePrice = input.BasePrice,
                Quantity = input.Quantity
            };

            _context.Inventories.Add(newProduct);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                isExisting = false,
                id = newProduct.Id,
                name = newProduct.Name,
                basePrice = newProduct.BasePrice,
                quantity = newProduct.Quantity,
                message = "New product added successfully!"
            });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AvailableInventories = _context.Inventories.ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            decimal total = 0;
            Invoice.InvoiceItems = new List<InvoiceItem>();

            foreach (var item in SelectedItems.Where(x => x.Quantity > 0))
            {
                var product = await _context.Inventories.FindAsync(item.InventoryId);

                if (product == null || product.Quantity < item.Quantity)
                {
                    ModelState.AddModelError("",
                        $"Not enough stock for {product?.Name ?? "Unknown"}");
                    return Page();
                }

                decimal finalPrice = item.CustomPrice > 0 ? item.CustomPrice : product.BasePrice;

                if (item.UpdateInventoryPrice && item.CustomPrice > 0 && item.CustomPrice != product.BasePrice)
                {
                    product.BasePrice = item.CustomPrice;
                }

                var newItem = new InvoiceItem
                {
                    InventoryId = product.Id,
                    Quantity = (int)item.Quantity,
                    UnitPrice = finalPrice,
                    SubTotal = item.Quantity * finalPrice
                };

                Invoice.InvoiceItems.Add(newItem);
                total += newItem.SubTotal;

                product.Quantity -= (int)Math.Floor((double)item.Quantity);
            }

            Invoice.TotalAmount = total;

            _context.Invoices.Add(Invoice);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { pageIndex = ReturnPage });
        }

        public class InvoiceItemInput
        {
            public int InventoryId { get; set; }
            // ✅ Float quantity support
            public decimal Quantity { get; set; }
            public decimal CustomPrice { get; set; }
            // ✅ Inventory price update checkbox
            public bool UpdateInventoryPrice { get; set; } = false;
        }

        public class QuickProductInput
        {
            public string Name { get; set; } = "";
            public decimal BasePrice { get; set; }
            public int Quantity { get; set; }
        }
    }
}
