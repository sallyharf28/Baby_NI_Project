namespace Project_TRAILLL.Services
{
    public interface IWatcherService
    {
        string directoryToMonitor { get; set; }
        string parserDir { get; set; } 
        void Main();

        
    }
}
