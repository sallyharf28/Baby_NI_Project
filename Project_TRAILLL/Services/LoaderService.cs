
using System;
using System.IO;
using System.Data;
using System.Data.Common;
using Vertica.Data.VerticaClient;
using Serilog;

namespace Project_TRAILLL.Services
{
    public class LoaderService : ILoaderService
    {
        private readonly IAggregationService _aggregationService;

        public LoaderService(IAggregationService aggregationService)
        {
            _aggregationService = aggregationService;
        }

        public void LoadCsvFile(string csvFilePath)
        
        {
            // Database connection string
            string connectionString = "Server=10.10.4.231;Database=test;User=bootcamp4;Password=bootcamp42023";

            // target table name
            string targetTableName;

            if (csvFilePath.Contains("SOEM1_TN_RADIO_LINK_POWER"))
            {
                targetTableName = "TRANS_MW_ERC_PM_TN_RADIO_LINK_POWER";
                
            }
            else if (csvFilePath.Contains("SOEM1_TN_RFInputPower"))
            {
                targetTableName = "TRANS_MW_ERC_PM_WAN_RFINPUTPOWER";
               
            }
            else
            {
                Console.WriteLine("Unrecognized CSV file. Skipped.");
                return;
            }
            
            try
            {
                using (VerticaConnection connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    using (VerticaCommand command = new VerticaCommand($"COPY {targetTableName} FROM LOCAL '{csvFilePath}' DELIMITER ',' skip 1;", connection))
                    {
                        //copies data from csv into tables
                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Loaded {csvFilePath} into {targetTableName} in Vertica.");
                    Log.Information($"Loaded {csvFilePath} into {targetTableName} in Vertica.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Log.Error($"Error: {ex.Message}");
            }

            
            _aggregationService.AggragateHourlyData();
            _aggregationService.AggregateDailyData();
        }
    }

}

