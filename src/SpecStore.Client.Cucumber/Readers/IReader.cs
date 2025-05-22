using Io.Cucumber.Messages.Types;

namespace SpecStore.Client.Cucumber.Readers
{
	internal interface IReader
	{
		IDictionary<string, string?> ReadMetadata(Meta meta);
		IEnumerable<UploadReportCommand.Feature> ReadFeatures(IEnumerable<Envelope> envelope);
	}
}
