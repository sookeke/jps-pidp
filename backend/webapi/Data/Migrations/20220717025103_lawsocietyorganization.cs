using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class lawsocietyorganization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LawSocietyLookup",
                columns: table => new
                {
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawSocietyLookup", x => x.Code);
                });

            migrationBuilder.InsertData(
                table: "LawSocietyLookup",
                columns: new[] { "Code", "Name" },
                values: new object[,] {
                    { 1, "BC Law Society" },
                    { 2, "Others" }
                });

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 3,
                column: "Name",
                value: "BC Law Society");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 4,
                column: "Name",
                value: "BC Corrections Service");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 5,
                column: "Name",
                value: "Health Authority");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 6,
                column: "Name",
                value: "BC Government Ministry");

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 7,
                column: "Name",
                value: "ICBC");

            migrationBuilder.InsertData(
                table: "OrganizationLookup",
                columns: new[] { "Code", "Name" },
                values: new object[] { 8, "Other" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LawSocietyLookup");

            migrationBuilder.DeleteData(
                table: "LawSocietyLookup",
                keyColumn: "Code",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 8);

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

            migrationBuilder.UpdateData(
                table: "OrganizationLookup",
                keyColumn: "Code",
                keyValue: 7,
                column: "Name",
                value: "Other");
        }
    }
}
