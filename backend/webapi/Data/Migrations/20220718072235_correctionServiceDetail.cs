using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class correctionServiceDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JuId",
                table: "JusticeSectorDetail",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "CorrectionServiceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CorrectionServiceCode = table.Column<int>(type: "integer", nullable: false),
                    PeronalId = table.Column<string>(type: "text", nullable: false),
                    OrgainizationDetailId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectionServiceDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectionServiceDetails_PartyOrgainizationDetail_Orgainiza~",
                        column: x => x.OrgainizationDetailId,
                        principalTable: "PartyOrgainizationDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectionServiceDetails_OrgainizationDetailId",
                table: "CorrectionServiceDetails",
                column: "OrgainizationDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrectionServiceDetails");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "JusticeSectorDetail",
                newName: "JuId");
        }
    }
}
