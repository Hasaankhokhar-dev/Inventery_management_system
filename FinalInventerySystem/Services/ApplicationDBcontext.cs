using FinalInventerySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalInventerySystem.Services
{
    public class ApplicationDBcontext : DbContext
    {
        public ApplicationDBcontext(DbContextOptions<ApplicationDBcontext> options)
            : base(options)
        {
        }

        // ✅ ADD THIS METHOD FOR SQLITE
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // ✅ SQLite configuration
                optionsBuilder.UseSqlite("Data Source=inventory.db");
            }
        }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Inventory> Inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ InvoiceItem relationships
            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Inventory)
                .WithMany()
                .HasForeignKey(ii => ii.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Ignore Adjustment properties in database
            modelBuilder.Entity<Invoice>()
                .Ignore(i => i.AdjustmentAmount)
                .Ignore(i => i.AdjustmentType);

            // ✅ Optional: Configure existing columns
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.Property(e => e.CustomerName).IsRequired();
                entity.Property(e => e.Phone).IsRequired();
                entity.Property(e => e.Address).IsRequired();
                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,2)");
            });

            // ✅ Optional: Configure Inventory
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);
                entity.Property(e => e.BasePrice)
                    .HasColumnType("decimal(16,2)");
            });
        }
    }
}