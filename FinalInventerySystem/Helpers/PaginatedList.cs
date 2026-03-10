
using Microsoft.EntityFrameworkCore;

namespace FinalInventerySystem.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // ✅ NEW: Smart paging logic
        public List<PageNumber> GetSmartPageNumbers()
        {
            var pages = new List<PageNumber>();

            // Always show first page
            pages.Add(new PageNumber(1, false));

            // Calculate range around current page
            int startPage = Math.Max(2, PageIndex - 1);
            int endPage = Math.Min(TotalPages - 1, PageIndex + 1);

            // Add ellipsis after first page if needed
            if (startPage > 2)
            {
                pages.Add(new PageNumber(0, true)); // Ellipsis
            }

            // Add pages around current page
            for (int i = startPage; i <= endPage; i++)
            {
                pages.Add(new PageNumber(i, false));
            }

            // Add ellipsis before last page if needed
            if (endPage < TotalPages - 1)
            {
                pages.Add(new PageNumber(0, true)); // Ellipsis
            }

            // Always show last page if there is more than 1 page
            if (TotalPages > 1)
            {
                pages.Add(new PageNumber(TotalPages, false));
            }

            return pages;
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

    // ✅ NEW: Page Number Model
    public class PageNumber
    {
        public int Number { get; set; }
        public bool IsEllipsis { get; set; }
        public string DisplayText => IsEllipsis ? "..." : Number.ToString();

        public PageNumber(int number, bool isEllipsis)
        {
            Number = number;
            IsEllipsis = isEllipsis;
        }
    }
}