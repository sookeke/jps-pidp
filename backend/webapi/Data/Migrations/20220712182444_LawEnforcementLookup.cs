using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class LawEnforcementLookup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParticipantId",
                table: "DigitalEvidence",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LawEnforcementLookup",
                columns: table => new
                {
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawEnforcementLookup", x => x.Code);
                });

            migrationBuilder.InsertData(
                table: "LawEnforcementLookup",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { 1, "Royal Canadian Mounted Police" },
                    { 2, "Victoria Police Department" },
                    { 3, "Sannich Police Department" },
                    { 4, "Delta Police Departmemt" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LawEnforcementLookup");

            migrationBuilder.DropColumn(
                name: "ParticipantId",
                table: "DigitalEvidence");
        }
    }
}
