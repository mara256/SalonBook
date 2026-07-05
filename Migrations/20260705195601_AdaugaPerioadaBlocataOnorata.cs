using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonBook.Migrations
{
    /// <inheritdoc />
    public partial class AdaugaPerioadaBlocataOnorata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recenzii_AspNetUsers_ClientId",
                table: "Recenzii");

            migrationBuilder.CreateTable(
                name: "PerioadeBlockate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalonId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataSfarsit = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Motiv = table.Column<string>(type: "TEXT", nullable: true),
                    DataCreare = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerioadeBlockate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerioadeBlockate_Saloane_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Saloane",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerioadeBlockate_SalonId",
                table: "PerioadeBlockate",
                column: "SalonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzii_AspNetUsers_ClientId",
                table: "Recenzii",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recenzii_AspNetUsers_ClientId",
                table: "Recenzii");

            migrationBuilder.DropTable(
                name: "PerioadeBlockate");

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzii_AspNetUsers_ClientId",
                table: "Recenzii",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
