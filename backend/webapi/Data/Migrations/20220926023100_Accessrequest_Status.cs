using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class Accessrequest_Status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AccessRequest",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "IdempotentConsumers",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "text", nullable: false),
                    Consumer = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotentConsumers", x => new { x.MessageId, x.Consumer });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdempotentConsumers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AccessRequest");
        }
    }
}
