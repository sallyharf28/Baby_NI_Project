using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging.Configuration;
using Project_TRAILLL.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//////
///
//var services = new ServiceCollection();

builder.Services.AddScoped <IWatcherService, WatcherService>();
builder.Services.AddScoped<IParserService,ParserService>();
builder.Services.AddScoped <ILoaderService,LoaderService>();
builder.Services.AddScoped <IAggregationService, AggregationService>(); 

//builder.Services.AddHttpLogging(httpLogging =>
//{
//    httpLogging.LoggingFields = HttpLoggingFields.All;
//});

var app = builder.Build();


//var serviceProvider = services.BuildServiceProvider();
//
//var watcherService = serviceProvider.GetRequiredService<IWatcherService>();
//watcherService.Main();
////////
//WatcherService watcherService = new WatcherService();
//watcherService.Main();



using (var scope = app.Services.CreateScope())
{
    var scopedServiceProvider = scope.ServiceProvider;
    var watcherService = scopedServiceProvider.GetRequiredService<IWatcherService>();
    watcherService.Main();

}
//var logger = new LoggerConfiguration()
//    .WriteTo.File("Logs/logger.txt",rollingInterval:RollingInterval.Hour)
//    .CreateLogger();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpLogging();   

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
