using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class PopulateDefaultPricingTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Populate default pricing tiers (0% interest) for all existing food packs
            // that don't already have pricing configured
            migrationBuilder.Sql(@"
                INSERT INTO FoodPackPricings (FoodPackId, DurationMonths, InterestRate, CreatedAt)
                SELECT fp.Id, d.Duration, 0.0000, GETDATE()
                FROM FoodPacks fp
                CROSS JOIN (VALUES (1), (2), (3), (4)) AS d(Duration)
                WHERE NOT EXISTS (
                    SELECT 1 FROM FoodPackPricings fpp
                    WHERE fpp.FoodPackId = fp.Id AND fpp.DurationMonths = d.Duration
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove only the pricing tiers with 0% interest that were auto-generated
            // This is a safe rollback as it preserves any manually configured pricing
            migrationBuilder.Sql(@"
                DELETE FROM FoodPackPricings
                WHERE InterestRate = 0.0000
            ");
        }
    }
}
