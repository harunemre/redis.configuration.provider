// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisConfigurationProvider;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddRedisConfiguration("SERVICE-A","localhost:6379", 1, null);

IHost host = builder.Build();

using var scope = host.Services.CreateAsyncScope();
var logger = scope.ServiceProvider.GetService<ILogger<Program>>();

var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();


var isBasketEnabled = configuration.GetValue<bool>("IsBasketEnabled");

logger.LogInformation("IsBasketEnabled: {IsBasketEnabled}", isBasketEnabled);

host.Run();
