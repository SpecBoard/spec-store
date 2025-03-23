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

		[Fact(DisplayName = "[UNIT][UPR-002]: Create project")]
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

		[Fact(DisplayName = "[UNIT][UPR-003]: Upload Report to Existing Project")]
		public async Task UploadReportCommand_PerformAsyn_UploadReportToExistingProject()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			await _database.InsertAsync(command.Project);

			// Act
			// Assert
			await sut.PerformAsync(command, default);
		}
	}

	file static class ReportPerformerTestExtensions
	{
		public static async Task<IEnumerable<ProjectEntity>> GetProjectsAsync(this DbContextOptions<ReportContext> database)
		{
			using var context = new ReportContext(database);
			return await context.Projects.ToListAsync();
		}

		public static async Task InsertAsync(this DbContextOptions<ReportContext> database, string project)
		{
			await using var context = new ReportContext(database);
			await using var transaction = await context.Database.BeginTransactionAsync();
			await context.AddAsync(new ProjectEntity { Key = project });
			await context.SaveChangesAsync();
			await transaction.CommitAsync();
		}
	}
}
