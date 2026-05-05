using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonBook.Migrations
{
    public partial class AdaugaClientiBlocati : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientiBlocati",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalonId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<string>(type: "TEXT", nullable: false),
                    DataBlocare = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Motiv = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientiBlocati", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientiBlocati_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientiBlocati_Saloane_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Saloane",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientiBlocati_ClientId",
                table: "ClientiBlocati",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientiBlocati_SalonId",
                table: "ClientiBlocati",
                column: "SalonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientiBlocati");
        }
    }
}
