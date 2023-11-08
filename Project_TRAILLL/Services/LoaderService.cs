
using System;
using System.IO;
using System.Data;
using System.Data.Common;
using Vertica.Data.VerticaClient;

namespace Project_TRAILLL.Services
{
    public class LoaderService
    {
        /*private string parserDir;
        private FileSystemWatcher fileWatcher;

        public LoaderService(string loaderDir)
        {
            this.parserDir = loaderDir;

            fileWatcher = new FileSystemWatcher(parserDir);
            fileWatcher.Filter = "*.csv";
            fileWatcher.Created += OnCsvFileCreated;
            fileWatcher.EnableRaisingEvents = true;
        }

        public void Start()
        {
            OnCsvFileCreated(this,null);
            Console.WriteLine("Loader service is running and monitoring for new CSV files.");

        }

        public void OnCsvFileCreated(object sender, FileSystemEventArgs e)
        {
            // Handle the new CSV file creation event
           
                string newCsvFile = e.FullPath;
                LoadCsvFile(newCsvFile);
               Console.WriteLine("hello its me");
            
            }
        */

        public void LoadCsvFile(string csvFilePath)
        
        {
            // Database connection string
            string connectionString = "Server=10.10.4.231;Database=test;User=bootcamp4;Password=bootcamp42023";

            // Define the target table name
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
                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Loaded {csvFilePath} into {targetTableName} in Vertica.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

}

