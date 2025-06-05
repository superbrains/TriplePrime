using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionFieldsToSavingsPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlanCode",
                table: "SavingsPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionCode",
                table: "SavingsPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanCode",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "SubscriptionCode",
                table: "SavingsPlans");
        }
    }
}
