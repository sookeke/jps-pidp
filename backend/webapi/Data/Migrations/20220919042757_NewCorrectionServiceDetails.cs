using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class NewCorrectionServiceDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CorrectionServiceDetails_CorrectionServiceLookup_Correction~",
                table: "CorrectionServiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_JusticeSectorDetail_JusticeSectorLookup_JusticeSectorCode",
                table: "JusticeSectorDetail");

            migrationBuilder.AlterColumn<int>(
                name: "JusticeSectorCode",
                table: "JusticeSectorDetail",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ParticipantId",
                table: "JusticeSectorDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "CorrectionServiceCode",
                table: "CorrectionServiceDetails",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_CorrectionServiceDetails_CorrectionServiceLookup_Correction~",
                table: "CorrectionServiceDetails",
                column: "CorrectionServiceCode",
                principalTable: "CorrectionServiceLookup",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_JusticeSectorDetail_JusticeSectorLookup_JusticeSectorCode",
                table: "JusticeSectorDetail",
                column: "JusticeSectorCode",
                principalTable: "JusticeSectorLookup",
                principalColumn: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CorrectionServiceDetails_CorrectionServiceLookup_Correction~",
                table: "CorrectionServiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_JusticeSectorDetail_JusticeSectorLookup_JusticeSectorCode",
                table: "JusticeSectorDetail");

            migrationBuilder.DropColumn(
                name: "ParticipantId",
                table: "JusticeSectorDetail");

            migrationBuilder.AlterColumn<int>(
                name: "JusticeSectorCode",
                table: "JusticeSectorDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CorrectionServiceCode",
                table: "CorrectionServiceDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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
    }
}
