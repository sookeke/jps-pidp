using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class AccessRequest_DigitialEvidence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserType",
                table: "DigitalEvidence",
                newName: "OrganizationType");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "DigitalEvidence",
                type: "text",
                nullable: false,
                defaultValue: "");

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "DigitalEvidence");

            migrationBuilder.RenameColumn(
                name: "OrganizationType",
                table: "DigitalEvidence",
                newName: "UserType");

        }
    }
}
