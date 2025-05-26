using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FoodPack> FoodPacks { get; set; }
        public DbSet<FoodPackItem> FoodPackItems { get; set; }
        public DbSet<SavingsPlan> SavingsPlans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Marketer> Marketers { get; set; }
        public DbSet<Commission> Commissions { get; set; }
        public DbSet<NotificationPreferences> NotificationPreferences { get; set; }
        public DbSet<DeliveryPreferences> DeliveryPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.NotificationPreferences)
                .WithMany()
                .HasForeignKey("NotificationPreferencesId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.DeliveryPreferences)
                .WithMany()
                .HasForeignKey("DeliveryPreferencesId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure UserRoles navigation property
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Make Address optional
            builder.Entity<ApplicationUser>()
                .Property(u => u.Address)
                .IsRequired(false);

            // Configure DeliveryAddress
            builder.Entity<DeliveryAddress>()
                .HasOne(da => da.User)
                .WithMany(u => u.DeliveryAddresses)
                .HasForeignKey(da => da.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Delivery
            builder.Entity<Delivery>()
                .HasOne(d => d.User)
                .WithMany(u => u.Deliveries)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Delivery>()
                .HasOne(d => d.Driver)
                .WithMany()
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Delivery>()
                .HasOne(d => d.DeliveryAddress)
                .WithMany()
                .HasForeignKey(d => d.DeliveryAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Delivery>()
                .HasOne(d => d.FoodPack)
                .WithMany()
                .HasForeignKey(d => d.FoodPackId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure FoodPack
            builder.Entity<FoodPack>()
                .HasOne(fp => fp.User)
                .WithMany()
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FoodPack>()
                .HasMany(fp => fp.Items)
                .WithOne(fpi => fpi.FoodPack)
                .HasForeignKey(fpi => fpi.FoodPackId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Payment
            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany()
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PaymentMethod
            builder.Entity<PaymentMethod>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.PaymentMethods)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Referral
            builder.Entity<Referral>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey("ReferrerId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Referral>()
                .HasOne(r => r.ReferredUser)
                .WithMany()
                .HasForeignKey(r => r.ReferredUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Report
            builder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Marketer configuration
            builder.Entity<Marketer>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Marketer>()
                .HasMany<Referral>()
                .WithOne()
                .HasForeignKey("MarketerId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Marketer>()
                .HasMany(m => m.Commissions)
                .WithOne(c => c.Marketer)
                .HasForeignKey(c => c.MarketerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Referral configuration
            builder.Entity<Referral>()
                .HasMany<Commission>()
                .WithOne()
                .HasForeignKey("ReferralId")
                .OnDelete(DeleteBehavior.Restrict);

            // Commission configuration
            builder.Entity<Commission>()
                .Property(c => c.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Commission>()
                .Property(c => c.Rate)
                .HasColumnType("decimal(5,2)");
        }
    }
} 