using System.Diagnostics;
using RedisConfigurationProvider;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddRedisConfiguration("SERVICE-B","localhost:6379", 1, null);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/configuration", (IConfiguration configuration) =>
{
    var siteName = configuration.GetValue<string>("SiteName");
    return siteName;
    
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();
