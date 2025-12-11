using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthCareManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStockToMedicine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Medicines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Medicines",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
