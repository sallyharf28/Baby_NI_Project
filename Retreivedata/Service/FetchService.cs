using Retreivedata.Models;
using System.Security.Policy;
using Vertica.Data.Internal.DotNetDSI;
using Vertica.Data.VerticaClient;

namespace Retreivedata.Service
{

    public class FetchService : IFetchService
    {
        public string connectionString { get; set; }


        //public FetchService(string connectionString)
        //{
        //    this.connectionString = connectionString;
        //}

        public List<List<AggregatedData>> GetAllAggregatedData()
        {
            List<List<AggregatedData>> allData = new List<List<AggregatedData>>();

            allData.Add(GetHourlyNETypeAggregatedData());
            allData.Add(GetHourlyNEAliasAggregatedData());

            allData.Add(GetDailyNeTypeAggregatedData());
            allData.Add(GetDailyNeAliasAggregatedData());

            return allData;

        }



        //HOURLY NEALIAS
        public List<AggregatedData> GetHourlyNETypeAggregatedData()
        {
            List<AggregatedData> result = new List<AggregatedData>();
            try
            {
                using (VerticaConnection connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    using (VerticaCommand command = new VerticaCommand("" +
                        "SELECT " +
                        "DATETIME_KEY, NETYPE,  MAX(RSL_INPUT_POWER) AS RSL_INPUT_POWER, MAX(MAX_RX_LEVEL) AS MAX_RX_LEVEL, " +
                        "CAST(ABS(MAX(RSL_INPUT_POWER))-ABS(MAX(MAX_RX_LEVEL))AS NUMERIC(10,2)) AS RSL_DEVIATION " +
                        "FROM TRANS_MW_AGG_SLOT_HOURLY GROUP BY 1,2;", connection))
                    {
                        using (VerticaDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AggregatedData data = new AggregatedData
                                {
                                    DateTimeKey = Convert.ToDateTime(reader["DATETIME_KEY"]),
                                    NeType = reader["NETYPE"].ToString(),
                                    RslInputPower = (float)Convert.ToDecimal(reader["RSL_INPUT_POWER"]),
                                    MaxRxLevel = (float)Convert.ToDecimal(reader["MAX_RX_LEVEL"]),
                                    RslDeviation = (float)Convert.ToDecimal(reader["RSL_DEVIATION"])
                                };
                                result.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching hourly Netype aggregated data: {ex.Message}");
            }
            return result;
        }

        ///HOURLY NEALIAS
        public List<AggregatedData> GetHourlyNEAliasAggregatedData()
        {
            List<AggregatedData> result = new List<AggregatedData>();
            try
            {
                using (VerticaConnection connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    using (VerticaCommand command = new VerticaCommand("SELECT DATETIME_KEY, NEALIAS,  MAX(RSL_INPUT_POWER) AS RSL_INPUT_POWER,   MAX(MAX_RX_LEVEL) AS MAX_RX_LEVEL, CAST(ABS(MAX(RSL_INPUT_POWER))-ABS(MAX(MAX_RX_LEVEL))AS NUMERIC(10,2)) AS RSL_DEVIATION FROM TRANS_MW_AGG_SLOT_HOURLY GROUP BY 1,2;", connection))
                    {
                        using (VerticaDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AggregatedData data = new AggregatedData
                                {
                                    DateTimeKey = Convert.ToDateTime(reader["DATETIME_KEY"]),
                                    NeAlias = reader["NEALIAS"].ToString(),
                                    RslInputPower = (float)Convert.ToDecimal(reader["RSL_INPUT_POWER"]),
                                    MaxRxLevel = (float)Convert.ToDecimal(reader["MAX_RX_LEVEL"]),
                                    RslDeviation = (float)Convert.ToDecimal(reader["RSL_DEVIATION"])
                                };
                                result.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching hourly NeAlias aggregated data: {ex.Message}");
            }

            return result;
        }

        
        ////DAILY NETYPE
        public List<AggregatedData> GetDailyNeAliasAggregatedData()
        {
            List<AggregatedData> result = new List<AggregatedData>();

            try
            {
                using (VerticaConnection connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    using (VerticaCommand command = new VerticaCommand("SELECT DATETIME_KEY, NEALIAS, MAX(RSL_INPUT_POWER) AS RSL_INPUT_POWER, MAX(MAX_RX_LEVEL) AS MAX_RX_LEVEL,CAST(ABS(MAX(RSL_INPUT_POWER))-ABS(MAX(MAX_RX_LEVEL))AS NUMERIC(10,2)) AS RSL_DEVIATION FROM TRANS_MW_AGG_SLOT_DAILY GROUP BY 1,2;", connection))
                    {
                        using (VerticaDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AggregatedData data = new AggregatedData
                                {
                                    DateTimeKey = Convert.ToDateTime(reader["DATETIME_KEY"]),
                                    NeAlias = reader["NEALIAS"].ToString(),
                                    RslInputPower = (float)Convert.ToDecimal(reader["RSL_INPUT_POWER"]),
                                    MaxRxLevel = (float)Convert.ToDecimal(reader["MAX_RX_LEVEL"]),
                                    RslDeviation = (float)Convert.ToDecimal(reader["RSL_DEVIATION"])
                                };

                                result.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching daily NeAlias aggregated data: {ex.Message}");
            }

            return result;
        }

        public List<AggregatedData> GetDailyNeTypeAggregatedData()
        {
            List<AggregatedData> result = new List<AggregatedData>();

            try
            {
                using (VerticaConnection connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    using (VerticaCommand command = new VerticaCommand("SELECT DATETIME_KEY, NETYPE, MAX(RSL_INPUT_POWER) AS RSL_INPUT_POWER, MAX(MAX_RX_LEVEL) AS MAX_RX_LEVEL,CAST(ABS(MAX(RSL_INPUT_POWER))-ABS(MAX(MAX_RX_LEVEL))AS NUMERIC(10,2)) AS RSL_DEVIATION FROM TRANS_MW_AGG_SLOT_DAILY GROUP BY 1,2;", connection))
                    {
                        using (VerticaDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AggregatedData data = new AggregatedData
                                {
                                    DateTimeKey = Convert.ToDateTime(reader["DATETIME_KEY"]),
                                    NeType = reader["NETYPE"].ToString(),
                                    RslInputPower = (float)Convert.ToDecimal(reader["RSL_INPUT_POWER"]),
                                    MaxRxLevel = (float)Convert.ToDecimal(reader["MAX_RX_LEVEL"]),
                                    RslDeviation = (float)Convert.ToDecimal(reader["RSL_DEVIATION"])
                                };

                                result.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching daily NeType aggregated data: {ex.Message}");
            }

            return result;
        }
    }
}
