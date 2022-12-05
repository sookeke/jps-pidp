using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pidp.Data.Migrations
{
    public partial class CrownRegionAuthorization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "AssignedRegions",
                table: "DigitalEvidence",
                type: "text[]",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "CrownRegionLookup",
                columns: table => new
                {
                    AgencyCode = table.Column<string>(type: "text", nullable: false),
                    CrownLocation = table.Column<string>(type: "text", nullable: false),
                    RegionName = table.Column<string>(type: "text", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrownRegionLookup", x => x.AgencyCode);
                });

            migrationBuilder.InsertData(
                table: "CrownRegionLookup",
                columns: new[] { "AgencyCode", "CrownLocation", "RegionId", "RegionName" },
                values: new object[,]
                {
                    { "205", "Vancouver Provincial Crown Counsel", 2, "Vancouver Region" },
                    { "6013", "Commercial Crime Crown Counsel", 6, "CASP" },
                    { "889", "Downtown Community Crown (Prov)", 2, "Vancouver Region" },
                    { "ATT", "Auto Theft Task Force Crown Counsel", 6, "CASP" },
                    { "BBCR", "Bella Bella Provincial Crown Counsel", 2, "Vancouver Region" },
                    { "BCCR", "Bella Coola Provincial Crown Counsel", 2, "Vancouver Region" },
                    { "C101", "Victoria Regional Crown Counsel", 1, "Vancouver Island Region" },
                    { "C102", "Victoria Youth Crown Counsel", 1, "Vancouver Island Region" },
                    { "C103", "Western Communities Crown Counsel", 1, "Vancouver Island Region" },
                    { "C104", "Courtenay Crown Counsel", 1, "Vancouver Island Region" },
                    { "C105", "Nanaimo Crown Counsel", 1, "Vancouver Island Region" },
                    { "C106", "Duncan Crown Counsel", 1, "Vancouver Island Region" },
                    { "C107", "Parksville (do not use) Crown Counsel", 1, "Vancouver Island Region" },
                    { "C108", "Port Alberni Crown Counsel", 1, "Vancouver Island Region" },
                    { "C109", "Campbell River Crown Counsel", 1, "Vancouver Island Region" },
                    { "C110", "Powell River Crown Counsel", 1, "Vancouver Island Region" },
                    { "C111", "Sidney Crown Counsel", 1, "Vancouver Island Region" },
                    { "C201", "Vancouver Regional Crown Counsel", 2, "Vancouver Region" },
                    { "C203", "Vancouver Traffic Crown Counsel", 2, "Vancouver Region" },
                    { "C204", "Vancouver Youth Crown Counsel", 2, "Vancouver Region" },
                    { "C205", "North Vancouver Crown Counsel", 2, "Vancouver Region" },
                    { "C206", "West Vancouver Crown Counsel", 2, "Vancouver Region" },
                    { "C207", "Richmond Crown Counsel", 2, "Vancouver Region" },
                    { "C208", "Sechelt Crown Counsel", 2, "Vancouver Region" },
                    { "C209", "Squamish Crown Counsel", 2, "Vancouver Region" },
                    { "C301", "New Westminster Regional Crown", 3, "Fraser Region" },
                    { "C302", "New Westminster Provincial Crown", 3, "Fraser Region" },
                    { "C303", "Burnaby Crown Counsel", 2, "Vancouver Region" },
                    { "C304", "Chilliwack Crown Counsel", 3, "Fraser Region" },
                    { "C305", "Abbotsford Crown Counsel", 3, "Fraser Region" },
                    { "C306", "Delta (Do Not Use) Crown Counsel", 3, "Fraser Region" },
                    { "C307", "Hope Crown Counsel", 3, "Fraser Region" },
                    { "C308", "Langley (Do Not Use) Crown Counsel", 3, "Fraser Region" },
                    { "C309", "Maple Ridge Crown Counsel", 3, "Fraser Region" },
                    { "C310", "Port Coquitlam Crown Counsel", 3, "Fraser Region" },
                    { "C311", "Surrey Crown Counsel", 3, "Fraser Region" },
                    { "C401", "Kamloops Crown Counsel", 4, "Interior Region" },
                    { "C403", "Penticton Crown Counsel", 4, "Interior Region" },
                    { "C404", "Vernon Crown Counsel", 4, "Interior Region" },
                    { "C405", "Cranbrook Crown Counsel", 4, "Interior Region" },
                    { "C406", "Nelson Crown Counsel", 4, "Interior Region" },
                    { "C501", "Prince George Crown Counsel", 5, "Northern Region" },
                    { "C502", "Dawson Creek Crown Counsel", 5, "Northern Region" },
                    { "C503", "Fort St John Crown Counsel", 5, "Northern Region" },
                    { "C504", "Prince Rupert Crown Counsel", 5, "Northern Region" },
                    { "C505", "Quesnel Crown Counsel", 5, "Northern Region" },
                    { "C506", "Smithers Crown Counsel", 5, "Northern Region" },
                    { "C507", "Terrace Crown Counsel", 5, "Northern Region" },
                    { "C508", "Vanderhoof Crown Counsel", 5, "Northern Region" },
                    { "C509", "Williams Lake Crown Counsel", 5, "Northern Region" },
                    { "C601", "CJB Headquarters - Victoria", 6, "CASP" },
                    { "C602", "CJB Headquarters - Vancouver", 6, "CASP" },
                    { "C603", "CJB Special Prosecutions", 6, "CASP" },
                    { "C604", "Port Hardy Crown Counsel", 1, "Vancouver Island Region" },
                    { "CCA5", "Centralized Charge Assessment Pilot - Region 5", 5, "Northern Region" },
                    { "CJHQ", "Criminal Justice Headquarters - RM", 6, "CASP" },
                    { "FNEL", "Fort Nelson Crown Counsel", 5, "Northern Region" },
                    { "HEF", "Health Fraud Crown Counsel", 6, "CASP" },
                    { "HUNM", "100 Mile House Crown Counsel", 5, "Northern Region" },
                    { "ICF", "ICBC Fraud Crown Counsel", 6, "CASP" },
                    { "kelo", "Kelowna Crown Counsel", 4, "Interior Region" },
                    { "KLCR", "Klemtu Provincial Crown Counsel", 2, "Vancouver Region" },
                    { "PEMB", "Pemberton Crown Counsel", 2, "Vancouver Region" },
                    { "PRC", "Proceeds of Crime Crown Counsel", 0, "CASP" },
                    { "SACC", "Salmon Arm Crown Counsel", 4, "Interior Region" },
                    { "WEF", "Welfare Fraud Crown Counsel", 6, "CASP" },
                    { "WOC", "Workers Compensation Crown Counsel", 6, "CASP" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrownRegionLookup");

            migrationBuilder.DropColumn(
                name: "AssignedRegions",
                table: "DigitalEvidence");
        }
    }
}
