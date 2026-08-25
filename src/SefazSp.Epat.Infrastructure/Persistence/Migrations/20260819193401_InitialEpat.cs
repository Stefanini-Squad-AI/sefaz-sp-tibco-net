using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SefazSp.Epat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialEpat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EpatSnapshots",
                columns: table => new
                {
                    StoreKind = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProcessId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DocumentJson = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpatSnapshots", x => new { x.StoreKind, x.ProcessId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EpatSnapshots");
        }
    }
}
