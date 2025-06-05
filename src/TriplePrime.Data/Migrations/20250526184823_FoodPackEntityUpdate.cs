using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class FoodPackEntityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodPacks_AspNetUsers_UserId",
                table: "FoodPacks");

            migrationBuilder.DropIndex(
                name: "IX_SavingsPlans_FoodPackId",
                table: "SavingsPlans");

            migrationBuilder.DropIndex(
                name: "IX_FoodPacks_UserId",
                table: "FoodPacks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FoodPacks");

            migrationBuilder.CreateTable(
                name: "FoodPackPurchases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FoodPackId = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodPackPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodPackPurchases_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FoodPackPurchases_FoodPacks_FoodPackId",
                        column: x => x.FoodPackId,
                        principalTable: "FoodPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavingsPlans_FoodPackId",
                table: "SavingsPlans",
                column: "FoodPackId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodPackPurchases_FoodPackId",
                table: "FoodPackPurchases",
                column: "FoodPackId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodPackPurchases_UserId",
                table: "FoodPackPurchases",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodPackPurchases");

            migrationBuilder.DropIndex(
                name: "IX_SavingsPlans_FoodPackId",
                table: "SavingsPlans");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FoodPacks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsPlans_FoodPackId",
                table: "SavingsPlans",
                column: "FoodPackId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodPacks_UserId",
                table: "FoodPacks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodPacks_AspNetUsers_UserId",
                table: "FoodPacks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
