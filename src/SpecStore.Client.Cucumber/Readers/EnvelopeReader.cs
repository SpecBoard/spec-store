using Io.Cucumber.Messages.Types;

namespace SpecStore.Client.Cucumber.Readers
{
	public class EnvelopeReader : IReader
	{
		public IDictionary<string, string?> ReadMetadata(Meta meta)
		{
			return new Dictionary<string, string?>()
			{
				["OS_NAME"] = meta.Os.Name,
				["OS_VERSION"] = meta.Os.Version,

				["CI_NAME"] = meta.Ci?.Name,
				["CI_BUILD_NUMBER"] = meta.Ci?.BuildNumber,
				["CI_URL"] = meta.Ci?.Url,

				["GIT_REMOTE"] = meta.Ci?.Git.Remote,
				["GIT_BRANCH"] = meta.Ci?.Git.Branch,
				["GIT_REVISION"] = meta.Ci?.Git.Revision,
				["GIT_TAG"] = meta.Ci?.Git.Tag,

				["RUNTIME_NAME"] = meta.Runtime.Name,
				["RUNTIME_VERSION"] = meta.Runtime.Version,

				["CPU_NAME"] = meta.Cpu.Name,
				["CPU_VERSION"] = meta.Cpu.Version,

				["IMPLEMENTATION_NAME"] = meta.Implementation.Name,
				["IMPLEMENTATION_VERSION"] = meta.Implementation.Version,

				["PROTOCOL_VERSION"] = meta.ProtocolVersion,
			};
		}
	}
}
