using Vertica.Data.VerticaClient;

namespace Project_TRAILLL.Services
{
    public class AggregationService : IAggregationService
    {
        public string connectionString = "Server=10.10.4.231;Database=test;User=bootcamp4;Password=bootcamp42023";
       
        public void AggragateHourlyData()
        {
            try
            {             
                using (VerticaConnection connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    if(TableExists(connection, "TRANS_MW_AGG_SLOT_HOURLY"))
                    {
                        using (VerticaCommand clearCommand = new VerticaCommand("DELETE FROM TRANS_MW_AGG_SLOT_HOURLY;", connection))
                        {
                            clearCommand.ExecuteNonQuery();
                            Console.WriteLine("Data Cleared Successfully.");
                        }

                    }
                    
                    using (VerticaCommand command = new VerticaCommand(@"INSERT INTO TRANS_MW_AGG_SLOT_HOURLY
                                                                         SELECT 
                                                                                DATE_TRUNC('HOUR', RADIO.TIME) AS ""TIME"",  
                                                                                RADIO.DATETIME_KEY,                                                                      
                                                                                RADIO.NETWORK_SID,
                                                                                RADIO.NEALIAS,
                                                                                RADIO.NETYPE,
                                                                                MAX(RFINPUT.RFINPUTPOWER) AS RSL_INPUT_POWER,
                                                                                MAX(RADIO.MAXRXLEVEL) AS MAX_RX_LEVEL,
                                                                                CAST(ABS(MAX(RFINPUT.RFINPUTPOWER))-ABS(MAX(RADIO.MAXRXLEVEL))AS NUMERIC(10,2)) AS RSL_DEVIATION
                                                                         FROM TRANS_MW_ERC_PM_TN_RADIO_LINK_POWER AS RADIO
                                                                         INNER JOIN TRANS_MW_ERC_PM_WAN_RFINPUTPOWER AS RFINPUT
                                                                                ON RFINPUT.NETWORK_SID = RADIO.NETWORK_SID 
                                                                         GROUP BY 1,2,3,4,5;", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine("Hourly aggregation completed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in hourly aggregation: {ex.Message}");
            }
        }
     
        public void AggregateDailyData()
        {
            try
            {
              
                using (VerticaConnection connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    if (TableExists(connection, "TRANS_MW_AGG_SLOT_DAILY"))
                    {
                        // Clear data from TRANS_MW_AGG_SLOT_DAILY
                        using (VerticaCommand clearCommand = new VerticaCommand("DELETE FROM TRANS_MW_AGG_SLOT_DAILY;", connection))
                        {
                            clearCommand.ExecuteNonQuery();
                        }
                    }

                    // Perform daily aggregation here
                    using (VerticaCommand command = new VerticaCommand(@"INSERT INTO TRANS_MW_AGG_SLOT_DAILY
                                                                        SELECT
                                                                                DATE_TRUNC('DAY', TIME) AS ""TIME"",
                                                                                DATETIME_KEY,                                                                         
                                                                                NETWORK_SID,
                                                                                NEALIAS,
                                                                                NETYPE,
                                                                                MAX(RSL_INPUT_POWER) AS RSL_INPUT_POWER,
                                                                                MAX(MAX_RX_LEVEL) AS MAX_RX_LEVEL,
                                                                                CAST(ABS(MAX(RSL_INPUT_POWER))-ABS(MAX(MAX_RX_LEVEL)) AS NUMERIC(10,2)) AS RSL_DEVIATION
                                                                        FROM TRANS_MW_AGG_SLOT_HOURLY
                                                                        GROUP BY 1,2,3,4,5;", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    Console.WriteLine("Daily aggregation completed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in daily aggregation: {ex.Message}");
            }
        }
        private bool TableExists(VerticaConnection connection, string tableName)
             {
                using (VerticaCommand command = new VerticaCommand($"SELECT COUNT(*) FROM tables WHERE table_name = '{tableName}';", connection))
                {
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
    }
}
