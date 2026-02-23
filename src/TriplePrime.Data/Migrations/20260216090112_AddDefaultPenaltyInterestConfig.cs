using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriplePrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultPenaltyInterestConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed default penalty interest configuration
            migrationBuilder.Sql(@"
                -- Daily interest rate (0.5% = 0.005)
                INSERT INTO Configurations ([Key], [Value], [Type], [Group], [Description], [IsEncrypted], [IsEnabled], [Environment], [ValidFrom], [CreatedAt], [CreatedBy])
                VALUES (
                    'PenaltyInterest:DailyRate',
                    '0.0050',
                    1,
                    'PenaltyInterest',
                    'Daily interest rate for overdue payments (0.50% per day)',
                    0,
                    1,
                    'Production',
                    GETUTCDATE(),
                    GETUTCDATE(),
                    'System'
                );

                -- Penalty interest enabled flag
                INSERT INTO Configurations ([Key], [Value], [Type], [Group], [Description], [IsEncrypted], [IsEnabled], [Environment], [ValidFrom], [CreatedAt], [CreatedBy])
                VALUES (
                    'PenaltyInterest:Enabled',
                    'true',
                    1,
                    'PenaltyInterest',
                    'Master switch to enable/disable penalty interest accrual (currently enabled)',
                    0,
                    1,
                    'Production',
                    GETUTCDATE(),
                    GETUTCDATE(),
                    'System'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove penalty interest configuration
            migrationBuilder.Sql(@"
                DELETE FROM Configurations WHERE [Key] = 'PenaltyInterest:DailyRate';
                DELETE FROM Configurations WHERE [Key] = 'PenaltyInterest:Enabled';
            ");
        }
    }
}
