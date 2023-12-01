using Retreivedata.Models;

namespace Retreivedata.Service
{
    public interface IFetchService
    {
        string connectionString { get; set; }
     
        List<List<AggregatedData>> GetAllAggregatedData();
        List<AggregatedData> GetHourlyNETypeAggregatedData();
        List<AggregatedData> GetHourlyNEAliasAggregatedData();     
        List<AggregatedData> GetDailyNeAliasAggregatedData();
        List<AggregatedData> GetDailyNeTypeAggregatedData();
        
    }
}
