using SpecStore.Client.Cucumber.Readers;
using SpecStore.Client.Cucumber.Serializers;
using SpecStore.Client.Cucumber.Shells;
using STrain;

namespace SpecStore.Client.Cucumber.Commands
{
	internal class UploadCommand
	{
		private readonly IShell _shell;
		private readonly IMessageSerializer _serializer;
		private readonly IReader _reader;
		private readonly IRequestSender _sender;

		public UploadCommand(IShell shell, IMessageSerializer serializer, IReader reader, IRequestSender sender)
		{
			_shell = shell;
			_serializer = serializer;
			_reader = reader;
			_sender = sender;
		}

		public async Task ExecuteAsync(string workingDirectory, CancellationToken cancellationToken)
		{
			var output = await _shell.ReportAsync(workingDirectory, cancellationToken);
			var envelopes = _serializer.Serialize(output);

			var metadata = _reader.ReadMetadata(envelopes.First(e => e.Meta is not null).Meta);
			var features = _reader.ReadFeatures(envelopes);

			await _sender.SendAsync(new UploadReportCommand { Project = "spec_board", Version = "1.0.0-dev.1", Metadata = metadata, Features = features }, cancellationToken);
		}
	}
}
