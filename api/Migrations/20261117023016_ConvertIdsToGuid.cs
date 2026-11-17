using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations;

public partial class ConvertIdsToGuid : Migration
{
    private static readonly (string Table, string Column, string Principal)[] Relationships =
    [
        ("TimetableSchedules", "BusinessId", "Businesses"),
        ("CustomerPackages", "BusinessId", "Businesses"),
        ("CustomerPackages", "CustomerId", "Customers"),
        ("Bookings", "CustomerPackageId", "CustomerPackages"),
        ("Bookings", "CustomerId", "Customers"),
        ("Bookings", "TimetableScheduleId", "TimetableSchedules"),
        ("WaitlistEntries", "CustomerPackageId", "CustomerPackages"),
        ("WaitlistEntries", "CustomerId", "Customers"),
        ("WaitlistEntries", "TimetableScheduleId", "TimetableSchedules")
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var (table, column, principal) in Relationships)
        {
            migrationBuilder.DropForeignKey($"FK_{table}_{principal}_{column}", table);
        }

        string[] tables = ["Businesses", "Customers", "CustomerPackages", "TimetableSchedules", "Bookings", "WaitlistEntries"];
        foreach (var table in tables)
        {
            // Remove AUTO_INCREMENT while the column is still an integer.
            migrationBuilder.Sql($"ALTER TABLE `{table}` MODIFY COLUMN `Id` int NOT NULL;");
            migrationBuilder.Sql($"ALTER TABLE `{table}` MODIFY COLUMN `Id` char(36) COLLATE ascii_general_ci NOT NULL;");
        }

        foreach (var (table, column, _) in Relationships)
        {
            migrationBuilder.Sql($"ALTER TABLE `{table}` MODIFY COLUMN `{column}` char(36) COLLATE ascii_general_ci NOT NULL;");
        }

        // Match the deterministic GUIDs in seed data without deleting existing rows.
        foreach (var table in tables)
        {
            ConvertColumn(migrationBuilder, table, "Id");
        }
        foreach (var (table, column, _) in Relationships)
        {
            ConvertColumn(migrationBuilder, table, column);
        }

        foreach (var (table, column, principal) in Relationships)
        {
            migrationBuilder.AddForeignKey(
                name: $"FK_{table}_{principal}_{column}", table: table, column: column,
                principalTable: principal, principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        }
    }

    private static void ConvertColumn(MigrationBuilder migrationBuilder, string table, string column)
    {
        migrationBuilder.Sql($"UPDATE `{table}` SET `{column}` = CONCAT('00000000-0000-0000-0000-', LPAD(`{column}`, 12, '0'));");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException("GUID IDs cannot safely be converted back to integers after new records are created. Restore a database backup to roll back this migration.");
    }
}
