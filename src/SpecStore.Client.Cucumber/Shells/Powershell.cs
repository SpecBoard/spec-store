using System.Diagnostics;

namespace SpecStore.Client.Cucumber.Shells
{
	internal class Powershell : IShell
	{
		public async Task<string> ReportAsync(string workingDirectory, CancellationToken cancellationToken)
		{
			using var process = Process.Start(new ProcessStartInfo
			{
				FileName = "powershell",
				Arguments = "npm run report",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,

				WorkingDirectory = workingDirectory,
			});

			var read = process.StandardOutput.ReadToEndAsync();
			var error = process.StandardError.ReadToEndAsync();

			await process.WaitForExitAsync(cancellationToken);

			if (process.ExitCode > 1)
			{
				throw new InvalidOperationException(await error);
			}

			var result = await read;
			return result.Substring(result.IndexOf('{'));
		}
	}
}