using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Models;

namespace RieltorCRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<Deal> Deals { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<Showing> Showings { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<DealHistory> DealHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка Id для Identity таблиц
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<IdentityRole<int>>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            // Связи для Deal
            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Agent)
                .WithMany(u => u.DealsAsAgent)
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Client)
                .WithMany(u => u.DealsAsClient)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Seller)
                .WithMany(u => u.DealsAsSeller)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Уникальный номер сделки
            modelBuilder.Entity<Deal>()
                .HasIndex(d => d.DealNumber)
                .IsUnique();

            // Индексы
            modelBuilder.Entity<Property>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Property>()
                .HasIndex(p => p.Price);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Status);

          
            modelBuilder.Entity<Property>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Property>()
                .Property(p => p.Area)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Deal>()
                .Property(d => d.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Deal>()
                .Property(d => d.Commission)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Deal>()
                .Property(d => d.CommissionPercent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasColumnType("decimal(18,2)");

           
            modelBuilder.Entity<Property>()
                .Property(p => p.Features)
                .HasColumnType("nvarchar(max)");

            
            modelBuilder.Entity<Showing>()
                .HasOne(s => s.Property)
                .WithMany(p => p.Showings)
                .HasForeignKey(s => s.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Showing>()
                .HasOne(s => s.Agent)
                .WithMany()
                .HasForeignKey(s => s.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Showing>()
                .HasOne(s => s.Client)
                .WithMany()
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

           
            modelBuilder.Entity<DealHistory>()
                .HasOne(h => h.Deal)
                .WithMany(d => d.History)
                .HasForeignKey(h => h.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DealHistory>()
                .HasOne(h => h.ChangedBy)
                .WithMany()
                .HasForeignKey(h => h.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}