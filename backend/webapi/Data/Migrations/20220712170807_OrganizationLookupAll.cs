using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class OrganizationLookupAll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 1,
                column: "Name",
                value: "Justice Sector");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 2,
                column: "Name",
                value: "BC Law Enforcement");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 3,
                column: "Name",
                value: "Law Society");

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

            migrationBuilder.InsertData(
                table: "OrganizationLookup",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { 6, "ICBC" },
                    { 7, "Other" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DeleteData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 7);

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 1,
                column: "Name",
                value: "Health Authority");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 2,
                column: "Name",
                value: "BC Government Ministry");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 3,
                column: "Name",
                value: "Maximus");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 4,
                column: "Name",
                value: "ICBC");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 5,
                column: "Name",
                value: "Other");
        }
    }
}
