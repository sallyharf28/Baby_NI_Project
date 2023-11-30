namespace Project_TRAILLL.Services
{
    public interface IWatcherService
    {
        string directoryToMonitor { get; set; }
        string parserDir { get; set; }
       // string archiveDir { get; set; }
        void Main();

        // void OnCreated(object sender, FileSystemEventArgs e);
    }
}
