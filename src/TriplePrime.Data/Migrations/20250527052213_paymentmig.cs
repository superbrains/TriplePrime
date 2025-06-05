using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class paymentmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_SavingsPlans_SavingsPlanId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SavingsPlanId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExpectedCompletionDate",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "SavingsPlanId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "MonthlyPayment",
                table: "SavingsPlans",
                newName: "MonthlyAmount");

            migrationBuilder.RenameColumn(
                name: "DurationInMonths",
                table: "SavingsPlans",
                newName: "Duration");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SavingsPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "SavingsPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "SavingsPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentPreference",
                table: "SavingsPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RemindersEnabled",
                table: "SavingsPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SavingsPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CallbackUrl",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PaymentSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SavingsPlanId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentSchedule_SavingsPlans_SavingsPlanId",
                        column: x => x.SavingsPlanId,
                        principalTable: "SavingsPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_SavingsPlanId",
                table: "PaymentSchedule",
                column: "SavingsPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "LastPaymentDate",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "PaymentPreference",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "RemindersEnabled",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "CallbackUrl",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "MonthlyAmount",
                table: "SavingsPlans",
                newName: "MonthlyPayment");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "SavingsPlans",
                newName: "DurationInMonths");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedCompletionDate",
                table: "SavingsPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SavingsPlanId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SavingsPlanId",
                table: "Payments",
                column: "SavingsPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_SavingsPlans_SavingsPlanId",
                table: "Payments",
                column: "SavingsPlanId",
                principalTable: "SavingsPlans",
                principalColumn: "Id");
        }
    }
}
