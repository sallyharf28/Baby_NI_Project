using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Retreivedata.Service;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IFetchService, FetchService>();
//FetchService fetchService = new FetchService();

builder.Services.AddCors(options =>

{

    options.AddDefaultPolicy(builder =>

    {

        builder.AllowAnyOrigin()

               .AllowAnyMethod()

               .AllowAnyHeader();

    });

});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapDefaultControllerRoute();
app.UseRouting();

app.UseCors();

app.Run();
