using TestAssesment.Data;

namespace TestAssesment.Utils
{
    public static class CsvUtils
    {
        public static void WriteCsv(List<TripData> records,string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("PickupDatetime,DropoffDatetime,PassengerCount,TripDistance,StoreAndFwdFlag,PULocationID,DOLocationID,FareAmount,TipAmount");

                foreach (var record in records)
                {
                    writer.WriteLine($"{record.TpepPickupDatetime},{record.TpepDropoffDatetime},{record.PassengerCount},{record.TripDistance},{record.StoreAndFwdFlag},{record.PULocationID},{record.DOLocationID},{record.FareAmount},{record.TipAmount}");
                }
            }

            Console.WriteLine($"Duplicates are recorded in file {filePath}");
        }
    }
}
