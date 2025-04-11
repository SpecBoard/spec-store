using LightInject;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SpecStore.Api;
using SpecStore.Application.Contexts;
using SpecStore.Application.Performers;
using SpecStore.Options;
using STrain;
using STrain.CQS.NetCore;
using STrain.CQS.NetCore.Builders;
using STrain.CQS.NetCore.LigtInject;
using STrain.Eventing.RabbitMQ.NetCore.Extensions;
using STrain.Eventing.RabbitMQ.Options;

namespace SpecStore.Wireup
{
	public static class Wireup
	{
		public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddOptionsWithValidateOnStart<DatabaseOptions>()
				.BindConfiguration("Database")
				.ValidateDataAnnotations();

			services.AddDbContext<ReportContext>((provider, builder) =>
			{
				var options = provider.GetRequiredService<IOptions<DatabaseOptions>>();
				builder.UseNpgsql(options.Value.ConnectionString);
			});

		}

		public static void ConfigureContainer(this IServiceRegistry registry)
		{

		}

		public static void ConfigureSTrain(this WebApplicationBuilder builder)
		{
			builder.AddCQS(builder =>
			{
				builder.AddPerformer<ICommandPerformer<UploadReportCommand>, ReportPerformers>();
				builder.AddPerformer<IQueryPerformer<GetProjectsQuery, IEnumerable<GetProjectsQuery.Result>>, ReportPerformers>();
				builder.AddPerformer<IQueryPerformer<GetProjectSummaryQuery, GetProjectSummaryQuery.Result>, ReportPerformers>();
				builder.AddPerformer<IQueryPerformer<GetProjectEvolutionQuery, IEnumerable<GetProjectEvolutionQuery.Result>>, ReportPerformers>();

				builder.AddMvcRequestReceiver()
					.UseLogger();
				builder.AddGenericRequestHandler("api");

				builder.AddRequestValidator()
					.UseFluentRequestValidator(builder => builder.RegistrateFrom(typeof(GetProjectEvolutionQueryValidator).Assembly));
			});

			builder.Services.RemoveAll<IProblemDetailsWriter>();
			builder.Services.AddExceptionHandler()
				.UseDefaultWriters();

			builder.AddEventing(builder =>
			{
				builder.AddPublisher(_ => "rabbitmq");

				builder.AddRabbitMQ((options, configuration) => configuration.Bind("RabbitMQ", options))
					.AddConnection("publish")
						.AddPublisher("rabbitmq");
			});
		}

		public static async Task InitializeAsync(this WebApplication application)
		{
			await application.Services.GetRequiredService<ReportContext>().Database.EnsureCreatedAsync();

			var options = application.Services.GetRequiredService<IOptions<RabbitMQOptions>>();
			var channel = application.Services.GetRequiredKeyedService<IChannel>("publish");

			await channel.ExchangeDeclareAsync(options.Value.Exchange, "topic", true, false);
		}
	}
}