using LightInject;
using STrain.CQS.NetCore;

namespace SpecStore.Wireup
{
	public static class Wireup
	{
		public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
		{

		}

		public static void ConfigureContainer(this IServiceRegistry registry)
		{

		}

		public static void ConfigureSTrain(this WebApplicationBuilder builder)
		{
			builder.AddCQS(builder => builder.AddMvcRequestReceiver());
		}
	}
}
