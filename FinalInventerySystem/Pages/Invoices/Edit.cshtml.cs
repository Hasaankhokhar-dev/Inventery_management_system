
using FinalInventerySystem.Models;
using FinalInventerySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalInventerySystem.Pages.Invoices
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDBcontext _context;
        private readonly IWebHostEnvironment _env;

        public EditModel(ApplicationDBcontext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Invoice Invoice { get; set; } = new();

        [BindProperty]
        public decimal AdjustmentAmount { get; set; }

        [BindProperty]
        public string AdjustmentType { get; set; } = "+";

        // ✅ NEW: Discount BindProperty
        [BindProperty]
        public decimal DiscountAmount { get; set; }

        public SelectList InventoryList { get; set; } = default!;
        public List<Inventory> Inventories { get; set; } = new();

        // ✅ GET ADJUSTMENT FROM JSON FILE
        private async Task<AdjustmentData> GetAdjustmentAsync(int invoiceId)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "AppData", "adjustments.json");

            if (!System.IO.File.Exists(filePath))
                return new AdjustmentData { InvoiceId = invoiceId, AdjustmentType = "+", DiscountAmount = 0 };

            try
            {
                var json = await System.IO.File.ReadAllTextAsync(filePath);
                var adjustments = JsonSerializer.Deserialize<Dictionary<int, AdjustmentData>>(json) ?? new Dictionary<int, AdjustmentData>();

                if (adjustments.ContainsKey(invoiceId))
                    return adjustments[invoiceId];

                return new AdjustmentData { InvoiceId = invoiceId, AdjustmentType = "+", DiscountAmount = 0 };
            }
            catch
            {
                return new AdjustmentData { InvoiceId = invoiceId, AdjustmentType = "+", DiscountAmount = 0 };
            }
        }

        // ✅ SAVE ADJUSTMENT TO JSON FILE - UPDATED WITH DISCOUNT
        private async Task SaveAdjustmentAsync(AdjustmentData adjustmentData)
        {
            var dataFolder = Path.Combine(_env.ContentRootPath, "AppData");
            var filePath = Path.Combine(dataFolder, "adjustments.json");

            // Ensure directory exists
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            Dictionary<int, AdjustmentData> adjustments;

            if (System.IO.File.Exists(filePath))
            {
                var json = await System.IO.File.ReadAllTextAsync(filePath);
                adjustments = JsonSerializer.Deserialize<Dictionary<int, AdjustmentData>>(json) ?? new Dictionary<int, AdjustmentData>();
            }
            else
            {
                adjustments = new Dictionary<int, AdjustmentData>();
            }

            adjustmentData.LastUpdated = DateTime.Now;
            adjustments[adjustmentData.InvoiceId] = adjustmentData;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var newJson = JsonSerializer.Serialize(adjustments, options);
            await System.IO.File.WriteAllTextAsync(filePath, newJson);
        }

        // GET: load invoice and inventory list
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .ThenInclude(ii => ii.Inventory)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (Invoice == null) return NotFound();

            // ✅ LOAD ADJUSTMENT FROM JSON
            var adjustmentData = await GetAdjustmentAsync(id);
            AdjustmentAmount = adjustmentData.AdjustmentAmount;
            AdjustmentType = string.IsNullOrEmpty(adjustmentData.AdjustmentType) ? "+" : adjustmentData.AdjustmentType;

            // ✅ NEW: Load DiscountAmount
            DiscountAmount = adjustmentData.DiscountAmount;

            var invs = await _context.Inventories
                .OrderBy(i => i.Name)
                .ToListAsync();

            Inventories = invs;
            InventoryList = new SelectList(invs, "Id", "Name");

            return Page();
        }

        // Helper to create a short code (Inventory.Code is required).
        private string GenerateInventoryCode()
        {
            return "P-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }

        // ✅ FIXED: POST METHOD WITH TRANSACTION AND VALIDATION - UPDATED WITH DISCOUNT
        public async Task<IActionResult> OnPostAsync()
        {
            // reload lists if validation fails
            var invsForList = await _context.Inventories.OrderBy(i => i.Name).ToListAsync();
            Inventories = invsForList;
            InventoryList = new SelectList(invsForList, "Id", "Name");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ✅ START TRANSACTION
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Load DB invoice including existing items
                var dbInvoice = await _context.Invoices
                    .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Inventory)
                    .FirstOrDefaultAsync(i => i.Id == Invoice.Id);

                if (dbInvoice == null)
                {
                    ModelState.AddModelError("", "Invoice not found!");
                    return Page();
                }

                // ✅ FIX: Ensure AdjustmentType is never null or empty
                if (string.IsNullOrEmpty(AdjustmentType))
                {
                    AdjustmentType = "+";
                }

                // ✅ SAVE ADJUSTMENT TO JSON FILE - UPDATED WITH DISCOUNT
                var adjustmentData = new AdjustmentData
                {
                    InvoiceId = Invoice.Id,
                    AdjustmentAmount = AdjustmentAmount,
                    AdjustmentType = AdjustmentType,
                    DiscountAmount = DiscountAmount  // ✅ NEW: Save discount
                };
                await SaveAdjustmentAsync(adjustmentData);

                // Update customer info only
                dbInvoice.CustomerName = Invoice.CustomerName;
                dbInvoice.Phone = Invoice.Phone;
                dbInvoice.Address = Invoice.Address;

                // ✅ FIXED: FILTER OUT INVALID ITEMS (InventoryId > 0 AND Quantity > 0)
                var validItems = Invoice.InvoiceItems
                    .Where(item => item.InventoryId > 0 && item.Quantity > 0)
                    .ToList();

                if (validItems.Count == 0)
                {
                    ModelState.AddModelError("", "At least one valid product item is required!");
                    return Page();
                }

                // 1) Restore old stock (rollback previous deduction)
                foreach (var oldItem in dbInvoice.InvoiceItems)
                {
                    var oldInv = await _context.Inventories.FindAsync(oldItem.InventoryId);
                    if (oldInv != null)
                    {
                        oldInv.Quantity += oldItem.Quantity;
                    }
                }

                // ✅ SAVE STOCK RESTORATION FIRST
                await _context.SaveChangesAsync();

                // 2) Remove old items from DB
                _context.InvoiceItems.RemoveRange(dbInvoice.InvoiceItems);
                await _context.SaveChangesAsync();

                // 3) Rebuild items from VALID posted items only
                dbInvoice.InvoiceItems = new List<InvoiceItem>();
                decimal productsTotal = 0m;

                for (int i = 0; i < validItems.Count; i++)
                {
                    var postedItem = validItems[i];

                    Inventory inventory = null!;

                    if (postedItem.InventoryId > 0)
                    {
                        // existing inventory chosen
                        inventory = await _context.Inventories.FindAsync(postedItem.InventoryId);
                        if (inventory == null)
                        {
                            await transaction.RollbackAsync();
                            ModelState.AddModelError("", $"Selected product (id {postedItem.InventoryId}) not found.");
                            return Page();
                        }
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Invalid product for row #{i + 1}.");
                        return Page();
                    }

                    // Check stock availability
                    if (inventory.Quantity < postedItem.Quantity)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Insufficient stock for {inventory.Name}. Available: {inventory.Quantity}");
                        return Page();
                    }

                    // Create invoice item
                    var newItem = new InvoiceItem
                    {
                        Inventory = inventory,
                        Quantity = postedItem.Quantity,
                        UnitPrice = postedItem.UnitPrice,
                        SubTotal = postedItem.UnitPrice * postedItem.Quantity
                    };

                    // Deduct stock
                    inventory.Quantity -= postedItem.Quantity;

                    dbInvoice.InvoiceItems.Add(newItem);
                    productsTotal += newItem.SubTotal;
                }

                // ✅ CALCULATE FINAL TOTAL WITH ADJUSTMENT AND DISCOUNT
                decimal finalTotal = productsTotal;

                // 1️⃣ Apply main adjustment (+ or -)
                if (AdjustmentType == "+")
                {
                    finalTotal += AdjustmentAmount;
                }
                else
                {
                    finalTotal -= AdjustmentAmount;
                }

                // 2️⃣ ✅ NEW: Apply discount (ALWAYS subtract)
                finalTotal -= DiscountAmount;

                // Ensure final total is not negative
                if (finalTotal < 0)
                {
                    finalTotal = 0;
                }

                // Update final total
                dbInvoice.TotalAmount = finalTotal;

                // ✅ FINAL SAVE AND COMMIT
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ SUCCESS MESSAGE
                TempData["SuccessMessage"] = "Invoice updated successfully!";
            }
            catch (Exception ex)
            {
                // ✅ AUTOMATIC ROLLBACK ON ERROR
                await transaction.RollbackAsync();

                Console.WriteLine($"Error updating invoice: {ex.Message}");

                ModelState.AddModelError("", $"An error occurred while updating the invoice. Please try again.");

                return Page();
            }

            return RedirectToPage("./Index");
        }
    }

    // ✅ ADJUSTMENT DATA CLASS - UPDATED WITH DISCOUNT
    public class AdjustmentData
    {
        public int InvoiceId { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public string AdjustmentType { get; set; } = "+";

        // ✅ NEW: Discount field - ALWAYS subtracts
        public decimal DiscountAmount { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}