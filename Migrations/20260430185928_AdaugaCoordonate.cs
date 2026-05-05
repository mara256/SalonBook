using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonBook.Migrations
{
    public partial class AdaugaCoordonate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitudine",
                table: "Saloane",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitudine",
                table: "Saloane",
                type: "REAL",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitudine",
                table: "Saloane");

            migrationBuilder.DropColumn(
                name: "Longitudine",
                table: "Saloane");
        }
    }
}
