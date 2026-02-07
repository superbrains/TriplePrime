using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalPricingTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Configurations table (if it doesn't exist)
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Group = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Environment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            // Add unique index on Key for fast lookups
            migrationBuilder.CreateIndex(
                name: "IX_Configurations_Key",
                table: "Configurations",
                column: "Key",
                unique: true);

            // Add UseGlobalRate column to FoodPackPricings (default true for new packs)
            migrationBuilder.AddColumn<bool>(
                name: "UseGlobalRate",
                table: "FoodPackPricings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Seed global pricing configuration with default rates (0%, 2%, 4%, 6%)
            // ConfigurationType.Application = 1
            migrationBuilder.Sql(@"
                INSERT INTO Configurations ([Key], [Value], [Type], [Group], [Description], IsEnabled, IsEncrypted, CreatedAt, CreatedBy, UpdatedBy, ValidFrom)
                VALUES
                  ('GlobalPricingTier:1Month', '0.00', 1, 'PricingTiers', 'Global interest rate for 1-month payment plans', 1, 0, GETDATE(), 'System', 'System', GETDATE()),
                  ('GlobalPricingTier:2Month', '0.02', 1, 'PricingTiers', 'Global interest rate for 2-month payment plans', 1, 0, GETDATE(), 'System', 'System', GETDATE()),
                  ('GlobalPricingTier:3Month', '0.04', 1, 'PricingTiers', 'Global interest rate for 3-month payment plans', 1, 0, GETDATE(), 'System', 'System', GETDATE()),
                  ('GlobalPricingTier:4Month', '0.06', 1, 'PricingTiers', 'Global interest rate for 4-month payment plans', 1, 0, GETDATE(), 'System', 'System', GETDATE());
            ");

            // Set existing food pack pricing to NOT use global (preserve current behavior)
            migrationBuilder.Sql(@"
                UPDATE FoodPackPricings SET UseGlobalRate = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove global pricing configurations
            migrationBuilder.Sql(@"
                DELETE FROM Configurations WHERE [Group] = 'PricingTiers';
            ");

            // Remove UseGlobalRate column
            migrationBuilder.DropColumn(
                name: "UseGlobalRate",
                table: "FoodPackPricings");

            // Drop the index
            migrationBuilder.DropIndex(
                name: "IX_Configurations_Key",
                table: "Configurations");

            // Drop Configurations table
            migrationBuilder.DropTable(
                name: "Configurations");
        }
    }
}
