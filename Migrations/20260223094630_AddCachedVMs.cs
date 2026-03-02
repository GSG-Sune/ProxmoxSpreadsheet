using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProxmoxDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddCachedVMs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSuccessfulFetch",
                table: "ProxmoxServers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CachedVMs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProxmoxServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    VMID = table.Column<int>(type: "INTEGER", nullable: false),
                    Node = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: false),
                    RAM = table.Column<int>(type: "INTEGER", nullable: false),
                    CPU = table.Column<string>(type: "TEXT", nullable: false),
                    Cores = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedVMs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CachedVMs_ProxmoxServers_ProxmoxServerId",
                        column: x => x.ProxmoxServerId,
                        principalTable: "ProxmoxServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CachedVMs_ProxmoxServerId",
                table: "CachedVMs",
                column: "ProxmoxServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedVMs");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulFetch",
                table: "ProxmoxServers");
        }
    }
}
