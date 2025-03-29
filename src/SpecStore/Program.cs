using LightInject;
using Microsoft.Extensions.Internal;
using Serilog;
using SpecStore.Wireup;
using STrain.CQS.NetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
	.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHealthChecks();

builder.Services.AddTransient<ISystemClock, SystemClock>();

builder.Host.UseLightInject();
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.ConfigureServices(builder.Configuration);
builder.Host.ConfigureContainer<IServiceRegistry>(registry => registry.ConfigureContainer());

builder.ConfigureSTrain();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHealthChecks("/.well-known/healthy");

app.UseAuthorization();

app.MapControllers();
app.MapGenericRequestController();


await app.InitializeAsync();

await app.RunAsync();
