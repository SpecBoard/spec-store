using SpecStore.Client.Cucumber.Commands;
using SpecStore.Client.Cucumber.Readers;
using SpecStore.Client.Cucumber.Serializers;
using SpecStore.Client.Cucumber.Shells;

namespace SpecStore.Client.Cucumber
{
	internal class ProcessManager
	{
		private const string UPLOAD = "upload";

		public async Task ExecuteAsync(params object[] args)
		{
			switch (args[0])
			{
				case UPLOAD:
					await new UploadCommand(new Powershell(), new MessageSerializer(), new EnvelopeReader()).ExecuteAsync(args.GetWorkingDirectory(), default);
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
