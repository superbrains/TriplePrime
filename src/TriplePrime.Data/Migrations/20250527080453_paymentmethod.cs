using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class paymentmethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "SavingsPlans");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "SavingsPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationCode",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Bank",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardType",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsPlans_PaymentMethodId",
                table: "SavingsPlans",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsPlans_PaymentMethods_PaymentMethodId",
                table: "SavingsPlans",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingsPlans_PaymentMethods_PaymentMethodId",
                table: "SavingsPlans");

            migrationBuilder.DropIndex(
                name: "IX_SavingsPlans_PaymentMethodId",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "SavingsPlans");

            migrationBuilder.DropColumn(
                name: "AuthorizationCode",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "Bank",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "CardType",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PaymentMethods");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "SavingsPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "PaymentMethods",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
