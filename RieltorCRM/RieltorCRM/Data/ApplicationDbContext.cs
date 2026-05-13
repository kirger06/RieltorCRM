using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Models;

namespace RieltorCRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext()
        {
        }

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


            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

               
            });

            modelBuilder.Entity<IdentityRole<int>>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<IdentityUserLogin<int>>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            });

            modelBuilder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
            });

            modelBuilder.Entity<IdentityUserToken<int>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });



            modelBuilder.Entity<Property>(entity =>
            {
                // Связь с Seller (Продавец)
                entity.HasOne(p => p.Seller)
                    .WithMany()
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Связь с Agent (Риелтор) - может быть null
                entity.HasOne(p => p.Agent)
                    .WithMany()
                    .HasForeignKey(p => p.AgentId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Индексы для поиска
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.Price);
                entity.HasIndex(p => p.City);
            });

      

            modelBuilder.Entity<Deal>(entity =>
            {
                // Связь с Property
                entity.HasOne(d => d.Property)
                    .WithMany()
                    .HasForeignKey(d => d.PropertyId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Связь с Agent (Риелтор)
                entity.HasOne(d => d.Agent)
                    .WithMany()
                    .HasForeignKey(d => d.AgentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Связь с Client (Клиент)
                entity.HasOne(d => d.Client)
                    .WithMany()
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Связь с Seller (Продавец)
                entity.HasOne(d => d.Seller)
                    .WithMany()
                    .HasForeignKey(d => d.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Связь с OfficeManager (может быть null)
                entity.HasOne(d => d.OfficeManager)
                    .WithMany()
                    .HasForeignKey(d => d.OfficeManagerId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Уникальный номер сделки
                entity.HasIndex(d => d.DealNumber).IsUnique();

                // Индекс по статусу
                entity.HasIndex(d => d.Status);
            });

    

            modelBuilder.Entity<Transaction>(entity =>
            {
                // Связь с Deal (может быть null)
                entity.HasOne(t => t.Deal)
                    .WithMany()
                    .HasForeignKey(t => t.DealId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Связь с CreatedBy (кто создал)
                entity.HasOne(t => t.CreatedBy)
                    .WithMany()
                    .HasForeignKey(t => t.CreatedById)
                    .OnDelete(DeleteBehavior.SetNull);

                // Связь с ConfirmedBy (бухгалтер, кто подтвердил)
                entity.HasOne(t => t.ConfirmedBy)
                    .WithMany()
                    .HasForeignKey(t => t.ConfirmedById)
                    .OnDelete(DeleteBehavior.SetNull);

                // Индекс по статусу
                entity.HasIndex(t => t.Status);

                // Индекс по дате
                entity.HasIndex(t => t.CreatedAt);
            });

    

            modelBuilder.Entity<Showing>(entity =>
            {
                // Связь с Property
                entity.HasOne(s => s.Property)
                    .WithMany()
                    .HasForeignKey(s => s.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Связь с Agent
                entity.HasOne(s => s.Agent)
                    .WithMany()
                    .HasForeignKey(s => s.AgentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Связь с Client
                entity.HasOne(s => s.Client)
                    .WithMany()
                    .HasForeignKey(s => s.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Document>(entity =>
            {
                // Связь с Deal (может быть null)
                entity.HasOne(d => d.Deal)
                    .WithMany()
                    .HasForeignKey(d => d.DealId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Связь с Property (может быть null)
                entity.HasOne(d => d.Property)
                    .WithMany()
                    .HasForeignKey(d => d.PropertyId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Связь с User (кто загрузил)
                entity.HasOne(d => d.UploadedBy)
                    .WithMany()
                    .HasForeignKey(d => d.UploadedById)
                    .OnDelete(DeleteBehavior.SetNull);
            });

        

            modelBuilder.Entity<Invoice>(entity =>
            {
                // Связь с Transaction (может быть null)
                entity.HasOne(i => i.Transaction)
                    .WithMany()
                    .HasForeignKey(i => i.TransactionId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Связь с Deal (может быть null)
                entity.HasOne(i => i.Deal)
                    .WithMany()
                    .HasForeignKey(i => i.DealId)
                    .OnDelete(DeleteBehavior.SetNull);
            });


            modelBuilder.Entity<DealHistory>(entity =>
            {
                // Связь с Deal
                entity.HasOne(h => h.Deal)
                    .WithMany()
                    .HasForeignKey(h => h.DealId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Связь с User (кто изменил)
                entity.HasOne(h => h.ChangedBy)
                    .WithMany()
                    .HasForeignKey(h => h.ChangedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}