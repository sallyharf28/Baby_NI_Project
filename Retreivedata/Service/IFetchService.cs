using Retreivedata.Models;

namespace Retreivedata.Service
{
    public interface IFetchService
    {
         string connectionString { get; set; }
        // List<AggregatedData> GetHourlyAggregatedData();
        List<List<AggregatedData>> GetAllAggregatedData();
        List<AggregatedData> GetHourlyNETypeAggregatedData();
        List<AggregatedData> GetHourlyNEAliasAggregatedData();
        // List<DailyAggregatedData> GetDailyAggregatedData();
        List<AggregatedData> GetDailyNeAliasAggregatedData();

        List<AggregatedData> GetDailyNeTypeAggregatedData();
        
    }
}
