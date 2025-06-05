using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Configuration
{
    public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
    {
        public void Configure(EntityTypeBuilder<Referral> builder)
        {
            // Configure the relationship between Referral and Marketer
            builder
                .HasOne(r => r.Marketer)
                .WithMany(m => m.Referrals)
                .HasForeignKey(r => r.MarketerId)
                .HasPrincipalKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the relationship between Referral and ReferredUser
            builder
                .HasOne(r => r.ReferredUser)
                .WithMany()
                .HasForeignKey(r => r.ReferredUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 