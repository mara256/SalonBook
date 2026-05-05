using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonBook.Migrations
{
    public partial class AdaugaStatusSalon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Saloane",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Saloane");
        }
    }
}
