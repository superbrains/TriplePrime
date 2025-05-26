using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class SecondMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_DeliveryPreferences_DeliveryPreferencesId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_NotificationPreferences_NotificationPreferencesId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "NotificationPreferencesId",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryPreferencesId",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_DeliveryPreferences_DeliveryPreferencesId",
                table: "AspNetUsers",
                column: "DeliveryPreferencesId",
                principalTable: "DeliveryPreferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_NotificationPreferences_NotificationPreferencesId",
                table: "AspNetUsers",
                column: "NotificationPreferencesId",
                principalTable: "NotificationPreferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_DeliveryPreferences_DeliveryPreferencesId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_NotificationPreferences_NotificationPreferencesId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "NotificationPreferencesId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryPreferencesId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_DeliveryPreferences_DeliveryPreferencesId",
                table: "AspNetUsers",
                column: "DeliveryPreferencesId",
                principalTable: "DeliveryPreferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_NotificationPreferences_NotificationPreferencesId",
                table: "AspNetUsers",
                column: "NotificationPreferencesId",
                principalTable: "NotificationPreferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
