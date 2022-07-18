using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class JusticeSectorLookup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JusticeSectorLookup",
                columns: table => new
                {
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JusticeSectorLookup", x => x.Code);
                });

            migrationBuilder.InsertData(
                table: "JusticeSectorLookup",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { 1, "BC Prosecution Service" },
                    { 2, "Road Safety BC" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JusticeSectorLookup");
        }
    }
}
