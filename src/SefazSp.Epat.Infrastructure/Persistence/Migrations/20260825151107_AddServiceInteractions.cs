using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SefazSp.Epat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceInteractions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Port = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RequestJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResponseJson = table.Column<string>(type: "TEXT", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    Failure = table.Column<string>(type: "TEXT", nullable: true),
                    At = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DurationMs = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceInteractions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInteractions_CorrelationId",
                table: "ServiceInteractions",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceInteractions");
        }
    }
}
