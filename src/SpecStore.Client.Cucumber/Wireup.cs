using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpecStore.Client.Cucumber.Commands;
using SpecStore.Client.Cucumber.Readers;
using SpecStore.Client.Cucumber.Serializers;
using SpecStore.Client.Cucumber.Shells;
using STrain;
using STrain.CQS.Http.RequestSending;
using STrain.CQS.Http.RequestSending.Binders;
using STrain.CQS.Http.RequestSending.Binders.Generic;
using STrain.CQS.Http.RequestSending.Readers;
using STrain.CQS.Senders;

namespace SpecStore.Client.Cucumber
{
	internal static class Wireup
	{
		public static void ConfigureServices(this IServiceCollection services, params string[] args)
		{
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddCommandLine(args).Build());

			services.AddHttpClient();
			services.AddRequestRouter(_ => "specstore");
			services.AddHttpRequestSender("specstore", (options, _) =>
			{
				options.BaseAddress = new Uri("http://localhost:5100/");
				options.Path = "api";
			});
			services.AddSingleton<Func<string, Func<IRequestSender>?>>((provider) => (key) => () => new HttpRequestSender(provider.GetRequiredService<IHttpClientFactory>().CreateClient(key), provider,
											provider.GetRequiredKeyedService<IRouteBinder>(key), provider.GetRequiredKeyedService<IMethodBinder>(key), provider.GetRequiredKeyedService<IQueryParameterBinder>(key),
											provider.GetRequiredKeyedService<IHeaderParameterBinder>(key), provider.GetRequiredKeyedService<IBodyParameterBinder>(key),
											provider.GetRequiredKeyedService<IResponseReaderProvider>(key), provider.GetRequiredKeyedService<IRequestErrorHandler>(key),
											provider.GetRequiredService<ILogger<HttpRequestSender>>()));

			services.AddKeyedTransient<IMethodBinder, GenericMethodBinder>("specstore");
			services.AddKeyedTransient<IRouteBinder>("specstore", (provider, key) => new GenericRouteBinder(provider.GetRequiredService<IOptionsSnapshot<HttpRequestSenderOptions>>().Get(key.ToString()).Path, provider.GetRequiredService<ILogger<GenericRouteBinder>>()));
			services.AddKeyedTransient<IHeaderParameterBinder, GenericHeaderParameterBinder>("specstore");
			services.AddKeyedTransient<IQueryParameterBinder, GenericQueryParameterBinder>("specstore");
			services.AddKeyedTransient<IBodyParameterBinder, GenericBodyParameterBinder>("specstore");

			services.AddKeyedTransient<IResponseReaderProvider>("specstore", (provider, _) =>
			{
				var result = new ResponseReaderRegistry();
				result.Registrate<JsonResponseReader>("application/json");
				return result;
			});
			services.AddKeyedTransient<IRequestErrorHandler, GenericRequestErrorHandler>("specstore");

			services.AddTransient<IShell, Powershell>();
			services.AddTransient<IMessageSerializer, MessageSerializer>();
			services.AddTransient<IReader, EnvelopeReader>();

			services.AddTransient<UploadCommand>();

			services.AddSingleton<ProcessManager>();
		}
	}
}
