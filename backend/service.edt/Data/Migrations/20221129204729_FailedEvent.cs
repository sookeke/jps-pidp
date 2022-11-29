using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace edt.service.Data.Migrations
{
    public partial class FailedEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailedEventLogs",
                schema: "edt",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Producer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsumerGroupId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsumerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedEventLogs", x => x.EventId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailedEventLogs",
                schema: "edt");
        }
    }
}
