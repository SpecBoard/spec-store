using Microsoft.Extensions.Logging;
using Moq;
using SpecStore.Client.Cucumber;
using SpecStore.Test.Unit.Fakers;
using Xunit.Abstractions;

namespace SpecStore.Test.Unit.Client.Cucumber
{
	public class ProcessManagerTest
	{
		private Mock<IServiceProvider> _providerMock = null!;

		private readonly ILogger<ProcessManager> _logger;

		public ProcessManagerTest(ITestOutputHelper outputHelper)
		{
			_logger = new LoggerFactory()
						  .AddXUnit(outputHelper)
						  .CreateLogger<ProcessManager>();
		}

		private ProcessManager CreateSUT()
		{
			_providerMock = new Mock<IServiceProvider>();

			return new ProcessManager(_providerMock.Object);
		}

		[Trait("Feature", "UPL - Upload Report")]
		[Fact(DisplayName = "[UNIT][PCM-001] - Unknown Command")]
		public async Task ProcessManager_ExecuteAsync_UnknownCommand()
		{
			// Arrange
			var sut = CreateSUT();

			// Act
			// Assert
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.ExecuteAsync(new ArgumentsFaker().Generate()));
		}
	}
}
