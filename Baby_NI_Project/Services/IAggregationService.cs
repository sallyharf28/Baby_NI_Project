namespace Project_TRAILLL.Services
{
    public interface IAggregationService
    {
      //  string connectionString { get; set; }
        void AggragateHourlyData();

        void AggregateDailyData();



    }
}
