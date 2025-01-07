using Microsoft.Data.SqlClient;
using System.Data;
using TestAssesment.Data;

namespace TestAssesment.Utils
{
    public static class DbUtils
    {
        static string DbName = "TestDatabase";
        static string connectionString = "Server=localhost,1433;User=sa;Password=YourStrong@Password;TrustServerCertificate=True;";
        public static void CreateDatabase()
        {
            string createDbQuery = $@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{DbName}')
                BEGIN
                    CREATE DATABASE {DbName};
                END";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(createDbQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Database '{DbName}' created successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static void CreateTable()
        {
            string createTableQuery = $@"
                USE {DbName};
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'trip_data')
                BEGIN
                    CREATE TABLE trip_data (
                        tpep_pickup_datetime DATETIME,
                        tpep_dropoff_datetime DATETIME,
                        passenger_count INT,
                        trip_distance DECIMAL(10, 2),
                        store_and_fwd_flag NVARCHAR(10),
                        PULocationID INT,
                        DOLocationID INT,
                        fare_amount DECIMAL(10, 2),
                        tip_amount DECIMAL(10, 2)
                    );
                END";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Table created successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void InsertData(List<TripData> records)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("TpepPickupDatetime", typeof(DateTime));
            dataTable.Columns.Add("TpepDropoffDatetime", typeof(DateTime));
            dataTable.Columns.Add("PassengerCount", typeof(int));
            dataTable.Columns.Add("TripDistance", typeof(float));
            dataTable.Columns.Add("StoreAndFwdFlag", typeof(string));
            dataTable.Columns.Add("PULocationID", typeof(int));
            dataTable.Columns.Add("DOLocationID", typeof(int));
            dataTable.Columns.Add("FareAmount", typeof(decimal));
            dataTable.Columns.Add("TipAmount", typeof(decimal));

            foreach (var record in records)
            {
                dataTable.Rows.Add(
                    record.TpepPickupDatetime,
                    record.TpepDropoffDatetime,
                    record.PassengerCount,
                    record.TripDistance,
                    record.StoreAndFwdFlag.Trim(),
                    record.PULocationID,
                    record.DOLocationID,
                    record.FareAmount,
                    record.TipAmount
                );
            }
            using (var connection = new SqlConnection($"Server=localhost,1433;Database={DbName};User=sa;Password=YourStrong@Password;TrustServerCertificate=True;"))
            {
                connection.Open();
                using (var bulkCopy = new SqlBulkCopy(connection))
                {

                    bulkCopy.DestinationTableName = "trip_data";
                    bulkCopy.BatchSize = 1000;
                    //bulkCopy.NotifyAfter = 1000;
                    //bulkCopy.SqlRowsCopied += (sender, e) =>
                    //{
                    //    Console.WriteLine($"{e.RowsCopied} rows copied.");
                    //};

                    try
                    {
                        bulkCopy.WriteToServer(dataTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during bulk insert: {ex.Message}");
                    }
                }
            }
        }

        public static void LongestTripDistance()
        {
            string query = $"USE {DbName} SELECT TOP 100 PULocationID, DOLocationID, trip_distance FROM trip_data ORDER BY trip_distance DESC";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            Console.WriteLine("Longest trip by distance");
                            Console.WriteLine("PULocationID | DOLocationID | trip_distance");
                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["PULocationID"]} | {reader["DOLocationID"]} | {reader["trip_distance"]}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void LongestTripTime()
        {
            string query = $"USE {DbName} SELECT TOP 100 PULocationID, DOLocationID," +
                $" DATEDIFF(MINUTE, tpep_pickup_datetime, tpep_dropoff_datetime) AS trip_time_min," +
                $" DATEDIFF(HOUR, tpep_pickup_datetime, tpep_dropoff_datetime) AS trip_time_hour" +
                $" FROM trip_data ORDER BY DATEDIFF(MINUTE, tpep_pickup_datetime, tpep_dropoff_datetime) DESC";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            Console.WriteLine("Longest trip by time");
                            Console.WriteLine("PULocationID | DOLocationID | trip_time_min | trip_time_hour");
                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["PULocationID"]} | {reader["DOLocationID"]} | {reader["trip_time_min"]} | {reader["trip_time_hour"]}") ;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void HighestAvgTips()
        {
            string query = $"USE {DbName} SELECT TOP 1 PULocationID, AVG(tip_amount) AS avg_tip_amount FROM trip_data GROUP BY PULocationID ORDER BY avg_tip_amount DESC";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            Console.WriteLine("Highest Average Tips");
                            Console.WriteLine("PULocationID | avg_tip_amount");
                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["PULocationID"]} | {reader["avg_tip_amount"]}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
