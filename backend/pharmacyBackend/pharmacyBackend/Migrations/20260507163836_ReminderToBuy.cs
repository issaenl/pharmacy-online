using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharmacyBackend.Migrations
{
    /// <inheritdoc />
    public partial class ReminderToBuy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackQuantity",
                table: "MedicationReminders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PillsPerDay",
                table: "MedicationReminders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PillsPerPack",
                table: "MedicationReminders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemindToBuyMethod",
                table: "MedicationReminders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackQuantity",
                table: "MedicationReminders");

            migrationBuilder.DropColumn(
                name: "PillsPerDay",
                table: "MedicationReminders");

            migrationBuilder.DropColumn(
                name: "PillsPerPack",
                table: "MedicationReminders");

            migrationBuilder.DropColumn(
                name: "RemindToBuyMethod",
                table: "MedicationReminders");
        }
    }
}
