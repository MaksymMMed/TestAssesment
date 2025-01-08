using LINQtoCSV;
using TestAssesment.Data;
using TestAssesment.Utils;

//run database on docker before running this code
//docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password" -p 1433:1433 --name mssql -d mcr.microsoft.com/mssql/server

string? csvFilePath = string.Empty;
bool isValidFile = false;

while (!isValidFile)
{
    Console.WriteLine("Enter the path to the CSV file:");
    csvFilePath = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(csvFilePath))
    {
        Console.WriteLine("Error: The file path cannot be empty. Please try again.");
        continue;
    }

    try
    {
        if (!File.Exists(csvFilePath))
        {
            Console.WriteLine($"Error: File not found at '{csvFilePath}'. Please try again.");
            continue;
        }

        if (Path.GetExtension(csvFilePath).ToLower() != ".csv")
        {
            Console.WriteLine("Error: The file must have a .csv extension. Please try again.");
            continue;
        }

        isValidFile = true;
        Console.WriteLine("File found and validated.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}. Please try again.");
    }
}

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
TimeZoneInfo estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
foreach (var record in distinct)
{
    record.TpepPickupDatetime = TimeZoneInfo.ConvertTimeToUtc(record.TpepPickupDatetime, estTimeZone);
    record.TpepDropoffDatetime = TimeZoneInfo.ConvertTimeToUtc(record.TpepDropoffDatetime, estTimeZone);
}

// If csv file is 10GB, I will use batch processing and apply parallel processing to speed up the process.
// Records will be divided into bathces of 50,000 rows, and I will apply Paralel.ForEach to them


// Db Actions
DbUtils.CreateDatabase();
DbUtils.CreateTable();
DbUtils.InsertData(distinct);


// Queries
Console.WriteLine("\n");
DbUtils.RowsCount();
Console.WriteLine("\n");
DbUtils.LongestTripDistance();
Console.WriteLine("\n");
DbUtils.LongestTripTime();
Console.WriteLine("\n");
DbUtils.HighestAvgTips();