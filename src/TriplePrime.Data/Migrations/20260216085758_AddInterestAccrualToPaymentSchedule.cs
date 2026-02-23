using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestAccrualToPaymentSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AccruedInterest",
                table: "PaymentSchedule",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "InterestAccrualStartDate",
                table: "PaymentSchedule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastInterestCalculationDate",
                table: "PaymentSchedule",
                type: "datetime2",
                nullable: true);

            // Initialize interest accrual dates for existing overdue schedules
            // This gives existing defaulters a fresh start - AccruedInterest remains 0
            // but the first InterestAccrualService run will calculate accumulated interest
            migrationBuilder.Sql(@"
                UPDATE PaymentSchedule
                SET InterestAccrualStartDate = DATEADD(day, 1, DueDate),
                    LastInterestCalculationDate = DATEADD(day, 1, DueDate),
                    AccruedInterest = 0
                WHERE Status = 'Pending'
                  AND DueDate < CAST(GETUTCDATE() AS DATE)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccruedInterest",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "InterestAccrualStartDate",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "LastInterestCalculationDate",
                table: "PaymentSchedule");
        }
    }
}
