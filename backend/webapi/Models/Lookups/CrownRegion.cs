namespace Pidp.Models.Lookups;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("CrownRegionLookup")]
public class CrownRegion
{
    [Key]
    public string AgencyCode { get; set; } = string.Empty;
    public string CrownLocation { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public int RegionId { get; set; }
}
public class CrownRegionDataGenerator : ILookupDataGenerator<CrownRegion>
{
    public IEnumerable<CrownRegion> Generate() => new[]
    {
        new CrownRegion { AgencyCode = "C109", CrownLocation = "Campbell River Crown Counsel", RegionName = "Vancouver Island Region", RegionId = 1},
        new CrownRegion { AgencyCode = "C104", CrownLocation = "Courtenay Crown Counsel", RegionName = "Vancouver Island Region",RegionId = 1},
        new CrownRegion { AgencyCode = "C106", CrownLocation = "Duncan Crown Counsel", RegionName = "Vancouver Island Region", RegionId = 1},
        new CrownRegion { AgencyCode = "C105", CrownLocation = "Nanaimo Crown Counsel", RegionName = "Vancouver Island Region", RegionId = 1},
        new CrownRegion { AgencyCode = "C107", CrownLocation = "Parksville (do not use) Crown Counsel", RegionName = "Vancouver Island Region", RegionId = 1},
        new CrownRegion { AgencyCode = "C108", CrownLocation = "Port Alberni Crown Counsel", RegionName = "Vancouver Island Region", RegionId = 1},
        new CrownRegion { AgencyCode = "C604", CrownLocation = "Port Hardy Crown Counsel", RegionName = "Vancouver Island Region",RegionId = 1},
        new CrownRegion { AgencyCode = "C110", CrownLocation = "Powell River Crown Counsel", RegionName = "Vancouver Island Region", RegionId = 1},
        new CrownRegion { AgencyCode = "C111", CrownLocation = "Sidney Crown Counsel", RegionName = "Vancouver Island Region", RegionId = 1},
        new CrownRegion { AgencyCode = "C101", CrownLocation = "Victoria Regional Crown Counsel", RegionName = "Vancouver Island Region",RegionId = 1},
        new CrownRegion { AgencyCode = "C102", CrownLocation = "Victoria Youth Crown Counsel", RegionName = "Vancouver Island Region",RegionId = 1},
        new CrownRegion { AgencyCode = "C103", CrownLocation = "Western Communities Crown Counsel", RegionName = "Vancouver Island Region", RegionId = 1},
        new CrownRegion { AgencyCode = "BBCR", CrownLocation = "Bella Bella Provincial Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "BCCR", CrownLocation = "Bella Coola Provincial Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C303", CrownLocation = "Burnaby Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "889", CrownLocation = "Downtown Community Crown (Prov)", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "KLCR", CrownLocation = "Klemtu Provincial Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C205", CrownLocation = "North Vancouver Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "PEMB", CrownLocation = "Pemberton Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C207", CrownLocation = "Richmond Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C208", CrownLocation = "Sechelt Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C209", CrownLocation = "Squamish Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "205", CrownLocation = "Vancouver Provincial Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C201", CrownLocation = "Vancouver Regional Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C203", CrownLocation = "Vancouver Traffic Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C204", CrownLocation = "Vancouver Youth Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C206", CrownLocation = "West Vancouver Crown Counsel", RegionName = "Vancouver Region", RegionId = 2},
        new CrownRegion { AgencyCode = "C305", CrownLocation = "Abbotsford Crown Counsel", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C304", CrownLocation = "Chilliwack Crown Counsel", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C306", CrownLocation = "Delta (Do Not Use) Crown Counsel", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C307", CrownLocation = "Hope Crown Counsel", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C308", CrownLocation = "Langley (Do Not Use) Crown Counsel", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C309", CrownLocation = "Maple Ridge Crown Counsel", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C302", CrownLocation = "New Westminster Provincial Crown", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C301", CrownLocation = "New Westminster Regional Crown", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C310", CrownLocation = "Port Coquitlam Crown Counsel", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C311", CrownLocation = "Surrey Crown Counsel", RegionName = "Fraser Region", RegionId = 3},
        new CrownRegion { AgencyCode = "C405", CrownLocation = "Cranbrook Crown Counsel", RegionName = "Interior Region", RegionId = 4},
        new CrownRegion { AgencyCode = "C401", CrownLocation = "Kamloops Crown Counsel", RegionName = "Interior Region", RegionId = 4},
        new CrownRegion { AgencyCode = "kelo", CrownLocation = "Kelowna Crown Counsel", RegionName = "Interior Region", RegionId = 4},
        new CrownRegion { AgencyCode = "C406", CrownLocation = "Nelson Crown Counsel", RegionName = "Interior Region", RegionId = 4},
        new CrownRegion { AgencyCode = "C403", CrownLocation = "Penticton Crown Counsel", RegionName = "Interior Region", RegionId = 4},
        new CrownRegion { AgencyCode = "SACC", CrownLocation = "Salmon Arm Crown Counsel", RegionName = "Interior Region", RegionId = 4},
        new CrownRegion { AgencyCode = "C404", CrownLocation = "Vernon Crown Counsel", RegionName = "Interior Region", RegionId = 4},
        new CrownRegion { AgencyCode = "HUNM", CrownLocation = "100 Mile House Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C502", CrownLocation = "Dawson Creek Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "FNEL", CrownLocation = "Fort Nelson Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C503", CrownLocation = "Fort St John Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C501", CrownLocation = "Prince George Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C504", CrownLocation = "Prince Rupert Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C505", CrownLocation = "Quesnel Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C506", CrownLocation = "Smithers Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C507", CrownLocation = "Terrace Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C508", CrownLocation = "Vanderhoof Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "C509", CrownLocation = "Williams Lake Crown Counsel", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "ATT", CrownLocation = "Auto Theft Task Force Crown Counsel", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "C602", CrownLocation = "CJB Headquarters - Vancouver", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "C601", CrownLocation = "CJB Headquarters - Victoria", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "C603", CrownLocation = "CJB Special Prosecutions", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "CCA5", CrownLocation = "Centralized Charge Assessment Pilot - Region 5", RegionName = "Northern Region", RegionId = 5},
        new CrownRegion { AgencyCode = "6013", CrownLocation = "Commercial Crime Crown Counsel", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "CJHQ", CrownLocation = "Criminal Justice Headquarters - RM", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "HEF", CrownLocation = "Health Fraud Crown Counsel", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "ICF", CrownLocation = "ICBC Fraud Crown Counsel", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "PRC", CrownLocation = "Proceeds of Crime Crown Counsel", RegionName = "CASP"},
        new CrownRegion { AgencyCode = "WEF", CrownLocation = "Welfare Fraud Crown Counsel", RegionName = "CASP", RegionId = 6},
        new CrownRegion { AgencyCode = "WOC", CrownLocation = "Workers Compensation Crown Counsel", RegionName = "CASP", RegionId = 6},
    };
}
