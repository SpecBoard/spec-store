using Cucumber.Messages;
using Io.Cucumber.Messages.Types;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpecStore.Client.Cucumber.Serializers
{
	public class MessageSerializer : IMessageSerializer
	{
		private readonly JsonSerializerOptions _options = new();

		public MessageSerializer()
		{
			_options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			_options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			_options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

			_options.Converters.Add(new CucumberMessageEnumConverter<AttachmentContentEncoding>());
			_options.Converters.Add(new CucumberMessageEnumConverter<PickleStepType>());
			_options.Converters.Add(new CucumberMessageEnumConverter<SourceMediaType>());
			_options.Converters.Add(new CucumberMessageEnumConverter<StepDefinitionPatternType>());
			_options.Converters.Add(new CucumberMessageEnumConverter<StepKeywordType>());
			_options.Converters.Add(new CucumberMessageEnumConverter<TestStepResultStatus>());
		}

		public IEnumerable<Envelope> Serialize(string value)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
			using var reader = new NdjsonMessageReader(stream, (v) => JsonSerializer.Deserialize<Envelope>(v, _options));

			return [.. reader];
		}
	}
}
