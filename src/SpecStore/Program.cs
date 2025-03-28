using LightInject;
using Serilog;
using SpecStore.Wireup;
using STrain.CQS.NetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

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
