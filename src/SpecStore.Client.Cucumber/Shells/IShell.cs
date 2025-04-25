namespace SpecStore.Client.Cucumber.Shells
{
	internal interface IShell
	{
		Task<string> ReportAsync(string workingDirectory, CancellationToken cancellationToken);
	}
}
