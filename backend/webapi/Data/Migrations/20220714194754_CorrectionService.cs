using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class CorrectionService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorrectionServiceLookup",
                columns: table => new
                {
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectionServiceLookup", x => x.Code);
                });

            migrationBuilder.InsertData(
                table: "CorrectionServiceLookup",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { 1, "In Custody" },
                    { 2, "Out Of Custody" },
                    { 3, "In and Out Of Custody" }
                });

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 3,
                column: "Name",
                value: "BC Corrections Service");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 4,
                column: "Name",
                value: "Health Authority");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 5,
                column: "Name",
                value: "BC Government Ministry");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 6,
                column: "Name",
                value: "ICBC");

            migrationBuilder.InsertData(
                table: "OrganizationLookup",
                columns: new[] { "Code", "Name" },
                values: new object[] { 7, "Other" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrectionServiceLookup");

            migrationBuilder.DeleteData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 7);

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 3,
                column: "Name",
                value: "Health Authority");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 4,
                column: "Name",
                value: "BC Government Ministry");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 5,
                column: "Name",
                value: "ICBC");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 6,
                column: "Name",
                value: "Other");
        }
    }
}
