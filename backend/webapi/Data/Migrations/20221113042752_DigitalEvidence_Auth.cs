using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Pidp.Models;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class DigitalEvidence_Auth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<AssignedRegion>>(
                name: "AssignedRegions",
                table: "DigitalEvidence",
                type: "jsonb",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedRegions",
                table: "DigitalEvidence");
        }
    }
}
