using Microsoft.EntityFrameworkCore;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using SpecStore.Test.Unit.Fakers;
using Spector.Api;

namespace SpecStore.Test.Unit.Requests
{
	[Trait("Feature", "MP - Managing Projects")]
	public partial class ReportPerformerTest
	{
		[Theory(DisplayName = "[UNIT][UPR-001]: Project is not defined")]
		[InlineData("")]
		[InlineData(null)]
		[InlineData("   ")]
		public async Task UploadReportCommand_ValidateAsync_ProjectIsNull(string? project)
		{
			// Arrange
			var sut = new UploadReportCommandValidator();

			// Act
			var result = await sut.ValidateAsync(new UploadReportFaker().Project(project).Generate(), default);

			// Assert
			Assert.False(result.IsValid);
		}

		[Theory(DisplayName = "[UNIT][UPR-002]: Version is not defined")]
		[InlineData("")]
		[InlineData(null)]
		[InlineData("   ")]
		public async Task UploadReportCommand_ValidateAsync_VersionIsNull(string? project)
		{
			// Arrange
			var sut = new UploadReportCommandValidator();

			// Act
			var result = await sut.ValidateAsync(new UploadReportFaker().Version(project).Generate(), default);

			// Assert
			Assert.False(result.IsValid);
		}

		[Fact(DisplayName = "[UNIT][UPR-003]: Create project")]
		public async Task UploadReportCommand_PerformAsyn_CreateProject()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			// Act
			await sut.PerformAsync(command, default);

			// Assert
			Assert.Collection(await _database.GetProjectsAsync(), p => p.Key = command.Project);
		}

		[Fact(DisplayName = "[UNIT][UPR-004]: Upload Report to Existing Project")]
		public async Task UploadReportCommand_PerformAsyn_UploadReportToExistingProject()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			await _database.InsertAsync(command.Project, command.Version);

			// Act
			// Assert
			await sut.PerformAsync(command, default);
		}

		[Fact(DisplayName = "[UNIT][UPR-004]: Create version")]
		public async Task UploadReportCommand_PerformAsyn_CreateVersion()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			// Act
			await sut.PerformAsync(command, default);

			// Assert
			Assert.Collection(await _database.GetVersionsAsync(command.Project), p => p.Version = command.Version);
		}

		[Fact(DisplayName = "[UNIT][UPR-005]: Upload Report to Existing Version")]
		public async Task UploadReportCommand_PerformAsyn_UploadReportToExistingVersion()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			await _database.InsertAsync(command.Project, command.Version);

			// Act
			await sut.PerformAsync(command, default);

			// Assert
			Assert.Collection(await _database.GetVersionsAsync(command.Project), p => p.Version = command.Version);
		}
	}

	file static class ReportPerformerTestExtensions
	{
		public static async Task<IEnumerable<ProjectEntity>> GetProjectsAsync(this DbContextOptions<ReportContext> database)
		{
			using var context = new ReportContext(database, new FakeClock());
			return await context.Projects.ToListAsync();
		}

		public static async Task<IEnumerable<VersionEntity>> GetVersionsAsync(this DbContextOptions<ReportContext> database, string project)
		{
			using var context = new ReportContext(database, new FakeClock());
			return [.. (await context.Projects.Include(p => p.Versions).FirstAsync(e => e.Key == project))!.Versions];
		}

		public static async Task InsertAsync(this DbContextOptions<ReportContext> database, string project, string version)
		{
			await using var context = new ReportContext(database, new FakeClock());
			await using var transaction = await context.Database.BeginTransactionAsync();
			var entity = new ProjectEntity { Key = project };
			await context.AddAsync(entity);
			await context.AddAsync(new VersionEntity { Project = entity, Version = version });
			await context.SaveChangesAsync();
			await transaction.CommitAsync();
		}
	}
}
