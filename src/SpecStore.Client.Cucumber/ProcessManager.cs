using Microsoft.Extensions.DependencyInjection;
using SpecStore.Client.Cucumber.Commands;

namespace SpecStore.Client.Cucumber
{
	public class ProcessManager
	{
		private const string UPLOAD = "upload";

		private readonly IServiceProvider _provider;

		public ProcessManager(IServiceProvider provider)
		{
			_provider = provider;
		}

		public async Task ExecuteAsync(params object[] args)
		{
			switch (args[0])
			{
				case UPLOAD:
					await _provider.GetRequiredService<UploadCommand>().ExecuteAsync(args.GetWorkingDirectory(), default);
					break;
				default:
					throw new InvalidOperationException($"Unkown command: {args[0]}");
			}
		}
	}

	file static class ProcessManagerExtensions
	{
		public static string GetWorkingDirectory(this object[] args)
		{
			var index = args.ToList().IndexOf("--working-directory");
			if (index < 0) return ".";

			var result = args[index + 1];
			if (result is null) throw new ArgumentException("Working directory is not defined");
			return result.ToString()!;
		}
	}
}
