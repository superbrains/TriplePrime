using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodPackPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodPackPricings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FoodPackId = table.Column<int>(type: "int", nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodPackPricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodPackPricings_FoodPacks_FoodPackId",
                        column: x => x.FoodPackId,
                        principalTable: "FoodPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodPackPricings_FoodPackId_DurationMonths",
                table: "FoodPackPricings",
                columns: new[] { "FoodPackId", "DurationMonths" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodPackPricings");
        }
    }
}
