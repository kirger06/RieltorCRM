using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Models;

namespace RieltorCRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.ToTable("AspNetUsers");

                entity.HasOne(u => u.Company)
                    .WithMany()
                    .HasForeignKey(u => u.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            modelBuilder.Entity<IdentityRole<int>>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.ToTable("AspNetRoles");
            });

            modelBuilder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.ToTable("AspNetUserRoles");
            });

            modelBuilder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("AspNetUserClaims");
            });

            modelBuilder.Entity<IdentityUserLogin<int>>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
                entity.ToTable("AspNetUserLogins");
            });

            modelBuilder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("AspNetRoleClaims");
            });

            modelBuilder.Entity<IdentityUserToken<int>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
                entity.ToTable("AspNetUserTokens");
            });

            modelBuilder.Entity<IdentityUserRole<int>>()
                .HasOne<IdentityRole<int>>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IdentityUserRole<int>>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IdentityUserClaim<int>>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IdentityUserLogin<int>>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IdentityRoleClaim<int>>()
                .HasOne<IdentityRole<int>>()
                .WithMany()
                .HasForeignKey(rc => rc.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IdentityUserToken<int>>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Companies");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Property>(entity =>
            {
                entity.ToTable("Properties");

                entity.HasOne(p => p.Seller)
                    .WithMany()
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                entity.HasOne(p => p.Agent)
                    .WithMany()
                    .HasForeignKey(p => p.AgentId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.Price);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");

                entity.HasOne(b => b.Property)
                    .WithMany()
                    .HasForeignKey(b => b.PropertyId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                entity.HasOne(b => b.Client)
                    .WithMany()
                    .HasForeignKey(b => b.ClientId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                entity.HasIndex(b => new { b.PropertyId, b.Status });
            });
        }
    }
}
