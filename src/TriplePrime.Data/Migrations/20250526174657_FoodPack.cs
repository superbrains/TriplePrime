using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class FoodPack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "FoodPacks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "FoodPackItems");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "FoodPackItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "FoodPackItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FoodPackItems");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "FoodPacks",
                newName: "Inventory");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "FoodPacks",
                newName: "Savings");

            migrationBuilder.RenameColumn(
                name: "PopularityScore",
                table: "FoodPacks",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "FoodPackItems",
                newName: "Item");

            migrationBuilder.AddColumn<bool>(
                name: "Available",
                table: "FoodPacks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "FoodPacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Featured",
                table: "FoodPacks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "FoodPacks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "FoodPacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Available",
                table: "FoodPacks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FoodPacks");

            migrationBuilder.DropColumn(
                name: "Featured",
                table: "FoodPacks");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "FoodPacks");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "FoodPacks");

            migrationBuilder.RenameColumn(
                name: "Savings",
                table: "FoodPacks",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "Inventory",
                table: "FoodPacks",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "FoodPacks",
                newName: "PopularityScore");

            migrationBuilder.RenameColumn(
                name: "Item",
                table: "FoodPackItems",
                newName: "Unit");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "FoodPacks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FoodPackItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FoodPackItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "FoodPackItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FoodPackItems",
                type: "datetime2",
                nullable: true);
        }
    }
}
