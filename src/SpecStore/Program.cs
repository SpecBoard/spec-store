using LightInject;
using SpecStore.Wireup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Host.UseLightInject();

Wireup.ConfigureServices(builder.Services, builder.Configuration);
builder.Host.ConfigureContainer<IServiceRegistry>(Wireup.ConfigureContainer);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
