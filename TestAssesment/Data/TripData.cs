using LINQtoCSV;

namespace TestAssesment.Data
{
    public class TripData
    {
        [CsvColumn(Name = "tpep_pickup_datetime", FieldIndex = 1)]
        public DateTime TpepPickupDatetime { get; set; }

        [CsvColumn(Name = "tpep_dropoff_datetime", FieldIndex = 2)]
        public DateTime TpepDropoffDatetime { get; set; }

        [CsvColumn(Name = "passenger_count", FieldIndex = 3)]
        public int PassengerCount { get; set; }

        [CsvColumn(Name = "trip_distance", FieldIndex = 4)]
        public float TripDistance { get; set; }

        [CsvColumn(Name = "store_and_fwd_flag", FieldIndex = 5)]
        public string StoreAndFwdFlag { get; set; }

        [CsvColumn(Name = "PULocationID", FieldIndex = 6)]
        public int PULocationID { get; set; }

        [CsvColumn(Name = "DOLocationID", FieldIndex = 7)]
        public int DOLocationID { get; set; }

        [CsvColumn(Name = "fare_amount", FieldIndex = 8)]
        public decimal FareAmount { get; set; }

        [CsvColumn(Name = "tip_amount", FieldIndex = 9)]
        public decimal TipAmount { get; set; }
    }
}
