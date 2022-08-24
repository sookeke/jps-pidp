using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class JusticeSectorAndCorrections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JusticeSectorDetail_JusticeSectorCode",
                table: "JusticeSectorDetail",
                column: "JusticeSectorCode");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectionServiceDetails_CorrectionServiceCode",
                table: "CorrectionServiceDetails",
                column: "CorrectionServiceCode");

            migrationBuilder.AddForeignKey(
                name: "FK_CorrectionServiceDetails_CorrectionServiceLookup_Correction~",
                table: "CorrectionServiceDetails",
                column: "CorrectionServiceCode",
                principalTable: "CorrectionServiceLookup",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JusticeSectorDetail_JusticeSectorLookup_JusticeSectorCode",
                table: "JusticeSectorDetail",
                column: "JusticeSectorCode",
                principalTable: "JusticeSectorLookup",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CorrectionServiceDetails_CorrectionServiceLookup_Correction~",
                table: "CorrectionServiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_JusticeSectorDetail_JusticeSectorLookup_JusticeSectorCode",
                table: "JusticeSectorDetail");

            migrationBuilder.DropIndex(
                name: "IX_JusticeSectorDetail_JusticeSectorCode",
                table: "JusticeSectorDetail");

            migrationBuilder.DropIndex(
                name: "IX_CorrectionServiceDetails_CorrectionServiceCode",
                table: "CorrectionServiceDetails");
        }
    }
}
