using Io.Cucumber.Messages.Types;
using SpecStore.Client.Cucumber.Readers;
using SpecStore.Test.Unit.Fakers;

namespace SpecStore.Test.Unit.Client.Cucumber
{
	public class EnvelopeReaderTest
	{
		private EnvelopeReader CreateSUT()
		{
			return new EnvelopeReader();
		}

		[Trait("Feature", "UPL - Upload Report")]
		[Fact(DisplayName = "[UNIT][EVR-001]: Read Metadata")]
		public void EnvelopeReader_ReadMetadata()
		{
			// Arrange
			var sut = CreateSUT();
			var meta = new MetaFaker().Generate();

			// Act
			var result = sut.ReadMetadata(meta);

			// Assert
			Assert.Collection(result, [.. meta.Inspect()]);
		}
	}

	file static class EnvelopeReaderTestExtensions
	{
		public static IEnumerable<Action<KeyValuePair<string, string>>> Inspect(this Meta meta)
		{
			return [
				p => { Assert.Equal("OS_NAME", p.Key); Assert.Equal(meta.Os.Name, p.Value); },
				p => { Assert.Equal("OS_VERSION", p.Key); Assert.Equal(meta.Os.Version, p.Value); },

				p => { Assert.Equal("RUNTIME_NAME", p.Key); Assert.Equal(meta.Runtime.Name, p.Value); },
				p => { Assert.Equal("RUNTIME_VERSION", p.Key); Assert.Equal(meta.Runtime.Version, p.Value); },

				p => { Assert.Equal("CPU_NAME", p.Key); Assert.Equal(meta.Cpu.Name, p.Value); },

				p => { Assert.Equal("IMPLEMENTATION_NAME", p.Key); Assert.Equal(meta.Implementation.Name, p.Value); },
				p => { Assert.Equal("IMPLEMENTATION_VERSION", p.Key); Assert.Equal(meta.Implementation.Version, p.Value); },

				p => { Assert.Equal("PROTOCOL_VERSION", p.Key); Assert.Equal(meta.ProtocolVersion, p.Value); },

				p => { Assert.Equal("CI_NAME", p.Key); Assert.Equal(meta.Ci.Name, p.Value); },
				p => { Assert.Equal("CI_BUILD_NUMBER", p.Key); Assert.Equal(meta.Ci.BuildNumber, p.Value); },
				p => { Assert.Equal("CI_URL", p.Key); Assert.Equal(meta.Ci.Url, p.Value); },

				p => { Assert.Equal("GIT_REMOTE", p.Key); Assert.Equal(meta.Ci.Git.Remote, p.Value); },
				p => { Assert.Equal("GIT_BRANCH", p.Key); Assert.Equal(meta.Ci.Git.Branch, p.Value); },
				p => { Assert.Equal("GIT_REVISION", p.Key); Assert.Equal(meta.Ci.Git.Revision, p.Value); },
				p => { Assert.Equal("GIT_TAG", p.Key); Assert.Equal(meta.Ci.Git.Tag, p.Value); },

				p => { Assert.Equal("CPU_VERSION", p.Key); Assert.Equal(meta.Cpu.Version, p.Value); },
				];
		}
	}
}
