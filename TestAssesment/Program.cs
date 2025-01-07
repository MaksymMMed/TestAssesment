using LINQtoCSV;
using TestAssesment.Data;
using TestAssesment.Utils;

//run db on docker
//docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password" -p 1433:1433 --name mssql -d mcr.microsoft.com/mssql/server

string csvFilePath = "../../../sample-cab-data.csv";

var csvFile = new CsvFileDescription
{
    SeparatorChar = ',',       
    FirstLineHasColumnNames = true,
    FileCultureName = "en-US",
    IgnoreUnknownColumns = true,
};

var csvContext = new CsvContext();
var records = csvContext.Read<TripData>(csvFilePath, csvFile).ToList();

// Write duplicates to a file
var duplicates = records
    .GroupBy(x => new { x.PassengerCount, x.TpepPickupDatetime, x.TpepDropoffDatetime})
    .Where(g => g.Count() > 1)
    .SelectMany(g => g)
    .ToList();

CsvUtils.WriteCsv(duplicates, "duplicates.csv");

// Remove duplicates
var distinct = records
    .GroupBy(x => new { x.PassengerCount, x.TpepPickupDatetime, x.TpepDropoffDatetime})
    .Select(g => g.First())
    .ToList();


// Normalize StoreAndFwdFlag
foreach (var record in distinct)
{
    record.StoreAndFwdFlag = string.IsNullOrEmpty(record.StoreAndFwdFlag)
    ? "No"
    : record.StoreAndFwdFlag.Trim() == "Y"
        ? "Yes"
        : "No";
}

// Convert to UTC
TimeZoneInfo ukraineZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev");
foreach (var record in distinct)
{
    record.TpepPickupDatetime = TimeZoneInfo.ConvertTimeToUtc(record.TpepPickupDatetime, ukraineZone);
    record.TpepDropoffDatetime = TimeZoneInfo.ConvertTimeToUtc(record.TpepDropoffDatetime, ukraineZone);
}


// Db Actions
DbUtils.CreateDatabase();
DbUtils.CreateTable();
DbUtils.InsertData(distinct);

// Queries
Console.WriteLine("\n");
DbUtils.LongestTripDistance();
Console.WriteLine("\n");
DbUtils.LongestTripTime();
Console.WriteLine("\n");
DbUtils.HighestAvgTips();