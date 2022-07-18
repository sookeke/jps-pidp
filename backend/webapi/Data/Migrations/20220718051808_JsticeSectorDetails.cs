using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class JsticeSectorDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeIdentifier",
                table: "PartyOrgainizationDetail");

            migrationBuilder.DropColumn(
                name: "HealthAuthorityCode",
                table: "PartyOrgainizationDetail");

            migrationBuilder.CreateTable(
                name: "JusticeSectorDetail",
                columns: table => new
                {
                    JuId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JusticeSectorCode = table.Column<int>(type: "integer", nullable: false),
                    JustinUserId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrgainizationDetailId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JusticeSectorDetail", x => x.JuId);
                    table.ForeignKey(
                        name: "FK_JusticeSectorDetail_PartyOrgainizationDetail_OrgainizationD~",
                        column: x => x.OrgainizationDetailId,
                        principalTable: "PartyOrgainizationDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JusticeSectorDetail_OrgainizationDetailId",
                table: "JusticeSectorDetail",
                column: "OrgainizationDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JusticeSectorDetail");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeIdentifier",
                table: "PartyOrgainizationDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HealthAuthorityCode",
                table: "PartyOrgainizationDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
