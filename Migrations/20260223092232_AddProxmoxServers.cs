using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProxmoxDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddProxmoxServers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProxmoxServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    BaseURL = table.Column<string>(type: "TEXT", nullable: false),
                    ApiToken = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxmoxServers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProxmoxServers");
        }
    }
}
