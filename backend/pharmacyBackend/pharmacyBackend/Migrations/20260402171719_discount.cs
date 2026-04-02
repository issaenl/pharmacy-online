using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharmacyBackend.Migrations
{
    /// <inheritdoc />
    public partial class discount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountPercentage",
                table: "Stocks",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Stocks");
        }
    }
}
