/////*using FinalInventerySystem.Models;

//using FinalInventerySystem.Models;
//using FinalInventerySystem.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace FinalInventerySystem.Pages.Invoices
//{
//    public class CreateModel : PageModel
//    {
//        private readonly ApplicationDBcontext _context;

//        public CreateModel(ApplicationDBcontext context)
//        {
//            _context = context;
//        }

//        [BindProperty]
//        public Invoice Invoice { get; set; } = new Invoice();

//        [BindProperty]
//        public List<InvoiceItemInput> SelectedItems { get; set; } = new();

//        public List<Inventory> AvailableInventories { get; set; } = new();

//        public void OnGet()
//        {
//            AvailableInventories = _context.Inventories.ToList();
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            AvailableInventories = _context.Inventories.ToList();

//            if (!ModelState.IsValid)
//            {
//                return Page();
//            }

//            decimal total = 0;
//            Invoice.InvoiceItems = new List<InvoiceItem>();

//            foreach (var item in SelectedItems.Where(x => x.Quantity > 0))
//            {
//                var product = await _context.Inventories.FindAsync(item.InventoryId);

//                if (product == null || product.Quantity < item.Quantity)
//                {
//                    ModelState.AddModelError("", $"Not enough stock for {product?.Name ?? "Unknown"}");
//                    return Page();
//                }

//                var newItem = new InvoiceItem
//                {
//                    InventoryId = product.Id,
//                    Quantity = item.Quantity,
//                    UnitPrice = product.BasePrice,
//                    SubTotal = item.Quantity * product.BasePrice
//                };

//                Invoice.InvoiceItems.Add(newItem);
//                total += newItem.SubTotal;

//                product.Quantity -= item.Quantity;
//            }

//            Invoice.TotalAmount = total;

//            _context.Invoices.Add(Invoice);
//            await _context.SaveChangesAsync();

//            return RedirectToPage("Index");
//        }

//        public class InvoiceItemInput
//        {
//            public int InventoryId { get; set; }
//            public int Quantity { get; set; }
//        }
//    }
//}            -----------working code----------






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

        // ? SIRF YEH PROPERTY ADD KARO
        [BindProperty(SupportsGet = true)]
        public int ReturnPage { get; set; } = 1;

        public void OnGet(int? returnPage)
        {
            AvailableInventories = _context.Inventories.ToList();

            // ? SIRF YEH LINE ADD KARO
            if (returnPage.HasValue) ReturnPage = returnPage.Value;
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
                    ModelState.AddModelError("", $"Not enough stock for {product?.Name ?? "Unknown"}");
                    return Page();
                }

                var newItem = new InvoiceItem
                {
                    InventoryId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.BasePrice,
                    SubTotal = item.Quantity * product.BasePrice
                };

                Invoice.InvoiceItems.Add(newItem);
                total += newItem.SubTotal;

                product.Quantity -= item.Quantity;
            }

            Invoice.TotalAmount = total;

            _context.Invoices.Add(Invoice);
            await _context.SaveChangesAsync();

            // ? SIRF YEH LINE CHANGE - returnPage USE KARO
            return RedirectToPage("Index", new { pageIndex = ReturnPage });
        }

        public class InvoiceItemInput
        {
            public int InventoryId { get; set; }
            public int Quantity { get; set; }
        }
    }
}