using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharmacyBackend.Migrations
{
    /// <inheritdoc />
    public partial class pharmacyphoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Pharmacies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Pharmacies");
        }
    }
}
