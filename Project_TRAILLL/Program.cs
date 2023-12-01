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


Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog(); //log on each request

builder.Services.AddScoped <IWatcherService, WatcherService>();
builder.Services.AddScoped<IParserService,ParserService>();
builder.Services.AddScoped <ILoaderService,LoaderService>();
builder.Services.AddScoped <IAggregationService, AggregationService>(); 

builder.Services.AddHttpLogging(httpLogging =>
{
    httpLogging.LoggingFields = HttpLoggingFields.All;
});

var app = builder.Build();




using (var scope = app.Services.CreateScope())
{
    var scopedServiceProvider = scope.ServiceProvider;
    var watcherService = scopedServiceProvider.GetRequiredService<IWatcherService>();
    watcherService.Main();

}
 //Log.Logger = new LoggerConfiguration()
 //   .WriteTo.File("Logs/logger.txt", rollingInterval: RollingInterval.Hour)
 //   .CreateLogger();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



//app.UseHttpLogging();   

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
