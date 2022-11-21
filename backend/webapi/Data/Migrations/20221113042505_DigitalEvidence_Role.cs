using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Pidp.Models;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class DigitalEvidence_Role : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedRegions",
                table: "DigitalEvidence");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<AssignedRegion>>(
                name: "AssignedRegions",
                table: "DigitalEvidence",
                type: "jsonb",
                nullable: false);
        }
    }
}
