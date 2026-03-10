//using FinalInventerySystem.Models;
//using FinalInventerySystem.Services;
//using FinalInventerySystem.Helpers;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.EntityFrameworkCore;

//namespace FinalInventerySystem.Pages.Inventories
//{
//    public class IndexModel : PageModel
//    {
//        private readonly ApplicationDBcontext _context;

//        public IndexModel(ApplicationDBcontext context)
//        {
//            _context = context;
//        }

//        public PaginatedList<Inventory> InventoryList { get; set; } = default!;

//        [BindProperty(SupportsGet = true)]
//        public string? SearchString { get; set; }   // ✅ search query ko hold karega

//        public async Task OnGetAsync(string? searchString, int? pageIndex)
//        {
//            int pageSize = 8;

//            IQueryable<Inventory> inventoryIQ = from i in _context.Inventories
//                                                orderby i.Name
//                                                select i;

//            if (!string.IsNullOrEmpty(searchString))
//            {
//                inventoryIQ = inventoryIQ.Where(i =>
//                    i.Name.Contains(searchString) ||
//                    i.Code.Contains(searchString));
//            }

//            InventoryList = await PaginatedList<Inventory>.CreateAsync(
//                inventoryIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
//        }
//    }
//}






using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using FinalInventerySystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FinalInventerySystem.Pages.Inventories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBcontext _context;

        public IndexModel(ApplicationDBcontext context)
        {
            _context = context;
        }

        public PaginatedList<Inventory> InventoryList { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task OnGetAsync(string? searchString, int? pageIndex)
        {
            int pageSize = 8;

            IQueryable<Inventory> inventoryIQ = from i in _context.Inventories
                                                orderby i.Name
                                                select i;

            if (!string.IsNullOrEmpty(searchString))
            {
                inventoryIQ = inventoryIQ.Where(i =>
                    i.Name.Contains(searchString) ||
                    i.Code.Contains(searchString));
            }

            InventoryList = await PaginatedList<Inventory>.CreateAsync(
                inventoryIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
        }
    }
}