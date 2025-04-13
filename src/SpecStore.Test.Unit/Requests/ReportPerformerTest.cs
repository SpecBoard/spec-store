using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SpecStore.Application.Contexts;
using SpecStore.Application.Performers;
using SpecStore.Test.Unit.Fakers;
using STrain.Eventing.Publishers;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace SpecStore.Test.Unit.Requests
{
	[Collection("ReportPerformers")]
	public partial class ReportPerformerTest : IDisposable
	{
		private readonly ILogger<ReportPerformers> _logger;
		private DbContextOptions<ReportContext> _database = null!;
		private SqliteConnection _connection = null!;
		private Mock<IPublisher> _publisherMock = null!;

		public ReportPerformerTest(ITestOutputHelper outputHelper)
		{
			_logger = new LoggerFactory()
						  .AddXUnit(outputHelper)
						  .CreateLogger<ReportPerformers>();
		}

		private ReportPerformers CreateSUT()
		{
			_connection = new SqliteConnection("Filename=:memory:");
			_connection.Open();

			_database = new DbContextOptionsBuilder<ReportContext>()
								.UseSqlite(_connection)
								.Options;

			using var context = new ReportContext(_database, new FakeClock());
			context.Database.EnsureCreated();

			_publisherMock = new Mock<IPublisher>();

			return new ReportPerformers(new ReportContext(_database, new FakeClock()), _publisherMock.Object, _logger);
		}

		public void Dispose()
		{
			_connection?.Close();
			_connection?.Dispose();
		}
	}
}
