using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class marketdetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commissions_Referrals_ReferralId",
                table: "Commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Commissions_Referrals_ReferralId1",
                table: "Commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_AspNetUsers_ReferrerId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Marketers_MarketerId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Marketers_MarketerId1",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_MarketerId1",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_ReferrerId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Marketers_UserId",
                table: "Marketers");

            migrationBuilder.DropIndex(
                name: "IX_Commissions_ReferralId1",
                table: "Commissions");

            migrationBuilder.DropColumn(
                name: "MarketerId1",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferrerId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferralId1",
                table: "Commissions");

            migrationBuilder.AlterColumn<string>(
                name: "MarketerId",
                table: "Referrals",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Marketers_UserId",
                table: "Marketers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commissions_Referrals_ReferralId",
                table: "Commissions",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Marketers_MarketerId",
                table: "Referrals",
                column: "MarketerId",
                principalTable: "Marketers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commissions_Referrals_ReferralId",
                table: "Commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Marketers_MarketerId",
                table: "Referrals");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Marketers_UserId",
                table: "Marketers");

            migrationBuilder.AlterColumn<int>(
                name: "MarketerId",
                table: "Referrals",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "MarketerId1",
                table: "Referrals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReferrerId",
                table: "Referrals",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferralId1",
                table: "Commissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_MarketerId1",
                table: "Referrals",
                column: "MarketerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferrerId",
                table: "Referrals",
                column: "ReferrerId");

            migrationBuilder.CreateIndex(
                name: "IX_Marketers_UserId",
                table: "Marketers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_ReferralId1",
                table: "Commissions",
                column: "ReferralId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Commissions_Referrals_ReferralId",
                table: "Commissions",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Commissions_Referrals_ReferralId1",
                table: "Commissions",
                column: "ReferralId1",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_AspNetUsers_ReferrerId",
                table: "Referrals",
                column: "ReferrerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Marketers_MarketerId",
                table: "Referrals",
                column: "MarketerId",
                principalTable: "Marketers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Marketers_MarketerId1",
                table: "Referrals",
                column: "MarketerId1",
                principalTable: "Marketers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
