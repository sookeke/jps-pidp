using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class Outbox_ExportedEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {          
            migrationBuilder.CreateTable(
                name: "OutBoxedExportedEvent",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false),
                    AggregateId = table.Column<string>(type: "text", nullable: false),
                    AggregateType = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    EventPayload = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutBoxedExportedEvent", x => new { x.EventId, x.AggregateId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropPrimaryKey(name: "PK_AccessRequests", table: "AccessRequest");
            //migrationBuilder.DropColumn(name: "Id", table: "AccessRequest");

            //migrationBuilder.AddColumn<int>(name: "Id", table: "AccessRequest", nullable: false);

            //migrationBuilder.AddPrimaryKey(name: "PK_AccessRequests", table: "AccessRequest", column: "Id");

            migrationBuilder.DropTable(
                name: "OutBoxedExportedEvent");

          
        }
    }
}
