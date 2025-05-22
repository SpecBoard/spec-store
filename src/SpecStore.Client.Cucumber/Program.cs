using Microsoft.Extensions.DependencyInjection;
using SpecStore.Client.Cucumber;

var services = new ServiceCollection();

services.AddLogging();

services.ConfigureServices();

await services.BuildServiceProvider().GetRequiredService<ProcessManager>().ExecuteAsync(args);