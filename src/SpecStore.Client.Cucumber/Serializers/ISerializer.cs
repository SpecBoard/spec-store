using Io.Cucumber.Messages.Types;

namespace SpecStore.Client.Cucumber.Serializers
{
	internal interface IMessageSerializer
	{
		IEnumerable<Envelope> Serialize(string message);
	}
}
