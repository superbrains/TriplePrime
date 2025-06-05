using Microsoft.EntityFrameworkCore.Migrations;

namespace TriplePrime.Data.Migrations
{
    public partial class RemoveFoodPackUserRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodPacks_AspNetUsers_UserId",
                table: "FoodPacks");

            migrationBuilder.DropIndex(
                name: "IX_FoodPacks_UserId",
                table: "FoodPacks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FoodPacks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FoodPacks",
                type: "nvarchar(450)",
                nullable: true);

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