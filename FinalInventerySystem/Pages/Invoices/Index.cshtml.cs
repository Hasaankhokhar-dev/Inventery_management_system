



using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalInventerySystem.Pages.Invoices
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBcontext _context;
        public IndexModel(ApplicationDBcontext context) => _context = context;

        public List<Invoice> Invoices { get; set; } = new();
        public string SearchString { get; set; } = "";
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync(string searchString, int pageIndex = 1, int pageSize = 8)
        {
            SearchString = searchString ?? "";
            var query = _context.Invoices.AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
                query = query.Where(i => i.CustomerName.Contains(SearchString));

            int totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            CurrentPage = pageIndex;

            // ✅ Only pull invoices from DB (TotalAmount already set in Create/Edit)
            Invoices = await query
                .OrderByDescending(i => i.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}


