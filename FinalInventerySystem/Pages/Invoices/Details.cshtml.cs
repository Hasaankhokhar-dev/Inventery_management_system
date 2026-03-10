using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalInventerySystem.Pages.Invoices
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDBcontext _context;
        private readonly IWebHostEnvironment _env;

        public DetailsModel(ApplicationDBcontext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public Invoice Invoice { get; set; } = new();
        public decimal AdjustmentAmount { get; set; }
        public string AdjustmentType { get; set; } = "+";

        // ? NEW: Payment Received Property (from DiscountAmount)
        public decimal PaymentReceived { get; set; }

        // ? FIXED: Renamed class to avoid ambiguity
        private async Task<InvoiceAdjustmentData> GetAdjustmentAsync(int invoiceId)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "AppData", "adjustments.json");

            if (!System.IO.File.Exists(filePath))
                return new InvoiceAdjustmentData { InvoiceId = invoiceId, AdjustmentType = "+", DiscountAmount = 0 };

            try
            {
                var json = await System.IO.File.ReadAllTextAsync(filePath);
                var adjustments = JsonSerializer.Deserialize<Dictionary<int, InvoiceAdjustmentData>>(json) ?? new Dictionary<int, InvoiceAdjustmentData>();

                if (adjustments.ContainsKey(invoiceId))
                    return adjustments[invoiceId];

                return new InvoiceAdjustmentData { InvoiceId = invoiceId, AdjustmentType = "+", DiscountAmount = 0 };
            }
            catch
            {
                return new InvoiceAdjustmentData { InvoiceId = invoiceId, AdjustmentType = "+", DiscountAmount = 0 };
            }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Inventory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Invoice == null)
                return NotFound();

            // ? LOAD ADJUSTMENT FROM JSON
            var adjustmentData = await GetAdjustmentAsync(id);
            AdjustmentAmount = adjustmentData.AdjustmentAmount;
            AdjustmentType = string.IsNullOrEmpty(adjustmentData.AdjustmentType) ? "+" : adjustmentData.AdjustmentType;

            // ? NEW: Load Payment Received from DiscountAmount
            PaymentReceived = adjustmentData.DiscountAmount;

            return Page();
        }

        // ? UPDATED: PDF Download Handler with Payment Received
        public async Task<IActionResult> OnGetDownloadPdfAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Inventory)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // ? LOAD ADJUSTMENT FOR PDF
            var adjustmentData = await GetAdjustmentAsync(id);
            var adjustmentAmount = adjustmentData.AdjustmentAmount;
            var adjustmentType = string.IsNullOrEmpty(adjustmentData.AdjustmentType) ? "+" : adjustmentData.AdjustmentType;

            // ? NEW: Load Payment Received for PDF
            var paymentReceived = adjustmentData.DiscountAmount;

            // Calculate items total
            var itemsTotal = invoice.InvoiceItems.Sum(i => i.SubTotal);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Text("AL-Madina Electric & Solar House").FontSize(20).Bold().AlignCenter();
                        col.Item().Text("Zeeshan: 0312-2037249 | Shaukat: 0342-1417132").FontSize(12).AlignCenter();
                        col.Item().Text($"Invoice #{invoice.Id}").FontSize(16).Bold().AlignCenter();
                        col.Item().PaddingBottom(10);
                    });

                    page.Content().Column(col =>
                    {
                        // Customer Information
                        col.Item().Border(1).Padding(10).Column(customerCol =>
                        {
                            customerCol.Item().Text($"Customer Name: {invoice.CustomerName}").FontSize(11);
                            customerCol.Item().Text($"Phone: {invoice.Phone}").FontSize(11);
                            customerCol.Item().Text($"Address: {invoice.Address}").FontSize(11);
                            customerCol.Item().Text($"Date: {invoice.CreatedAt.ToString("dd/MM/yyyy hh:mm tt")}").FontSize(11);
                        });

                        col.Item().PaddingVertical(10);

                        // Items Table - Unit Price Column Removed
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Product - 60%
                                columns.ConstantColumn(60); // Qty - 20%
                                columns.ConstantColumn(100); // Subtotal - 20%
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Product").Bold().FontSize(10);
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qty").Bold().FontSize(10).AlignCenter();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Sub Total").Bold().FontSize(10).AlignRight();
                            });

                            // Table Rows
                            foreach (var item in invoice.InvoiceItems)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Inventory.Name).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Quantity.ToString()).FontSize(9).AlignCenter();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"Rs. {item.SubTotal.ToString("N0")}").FontSize(9).AlignRight();
                            }
                        });

                        col.Item().PaddingVertical(10);

                        // ? UPDATED: Totals with Adjustment AND Payment Received
                        col.Item().AlignRight().Column(totalsCol =>
                        {
                            // Items Total
                            totalsCol.Item().Width(250).Row(row =>
                            {
                                row.RelativeItem().Text("Items Total:").FontSize(11).SemiBold();
                                row.ConstantItem(100).Text($"Rs. {itemsTotal.ToString("N0")}").FontSize(11).AlignRight();
                            });

                            // Adjustment
                            totalsCol.Item().Width(250).Row(row =>
                            {
                                row.RelativeItem().Text("Adjustment:").FontSize(11).SemiBold();
                                row.ConstantItem(100).Text(
                                    adjustmentAmount > 0
                                        ? $"{(adjustmentType == "+" ? "+" : "-")} Rs. {adjustmentAmount.ToString("N0")}"
                                        : "Rs. 0"
                                ).FontSize(11).AlignRight();
                            });

                            // ? NEW: Payment Received Row
                            totalsCol.Item().Width(250).PaddingBottom(5).Row(row =>
                            {
                                row.RelativeItem().Text("Payment Received:").FontSize(11).SemiBold().FontColor(Colors.Green.Darken2);
                                row.ConstantItem(100).Text(
                                    paymentReceived > 0
                                        ? $"- Rs. {paymentReceived.ToString("N0")}"
                                        : "Rs. 0"
                                ).FontSize(11).AlignRight().FontColor(Colors.Green.Darken2);
                            });

                            // Final Total
                            totalsCol.Item().Width(250).BorderTop(1).PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Final Total:").FontSize(12).Bold();
                                row.ConstantItem(100).Text($"Rs. {invoice.TotalAmount.ToString("N0")}").FontSize(12).Bold().AlignRight();
                            });
                        });

                        col.Item().PaddingVertical(15);

                        // Footer
                        col.Item().BorderTop(1).PaddingTop(10).AlignCenter().Text("Thank you for your business!").FontSize(10).Italic();
                        col.Item().AlignCenter().Text($"Generated on: {DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")}").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Invoice_{invoice.Id}.pdf");
        }
    }

    // ? FIXED: Renamed class with DiscountAmount added
    public class InvoiceAdjustmentData
    {
        public int InvoiceId { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public string AdjustmentType { get; set; } = "+";

        // ? NEW: Discount Amount (Payment Received)
        public decimal DiscountAmount { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}