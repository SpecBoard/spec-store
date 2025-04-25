using SpecStore.Client.Cucumber.Readers;
using SpecStore.Client.Cucumber.Serializers;
using SpecStore.Client.Cucumber.Shells;

namespace SpecStore.Client.Cucumber.Commands
{
	internal class UploadCommand
	{
		private readonly IShell _shell;
		private readonly IMessageSerializer _serializer;
		private readonly IReader _reader;

		public UploadCommand(IShell shell, IMessageSerializer serializer, IReader reader)
		{
			_shell = shell;
			_serializer = serializer;
			_reader = reader;
		}

		public async Task ExecuteAsync(string workingDirectory, CancellationToken cancellationToken)
		{
			var output = await _shell.ReportAsync(workingDirectory, cancellationToken);
			var envelopes = _serializer.Serialize(output);

			var metadata = _reader.ReadMetadata(envelopes.First(e => e.Meta is not null).Meta);
		}
	}
}
