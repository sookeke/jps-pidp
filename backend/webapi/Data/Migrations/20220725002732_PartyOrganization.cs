using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class PartyOrganization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PartyOrgainizationDetail_OrganizationCode",
                table: "PartyOrgainizationDetail",
                column: "OrganizationCode");

            migrationBuilder.AddForeignKey(
                name: "FK_PartyOrgainizationDetail_OrganizationLookup_OrganizationCode",
                table: "PartyOrgainizationDetail",
                column: "OrganizationCode",
                principalTable: "OrganizationLookup",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyOrgainizationDetail_OrganizationLookup_OrganizationCode",
                table: "PartyOrgainizationDetail");

            migrationBuilder.DropIndex(
                name: "IX_PartyOrgainizationDetail_OrganizationCode",
                table: "PartyOrgainizationDetail");
        }
    }
}
