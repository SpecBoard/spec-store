using Microsoft.EntityFrameworkCore;
using SpecStore.Api;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using SpecStore.Test.Unit.Fakers;
using STrain.Core.Exceptions;

namespace SpecStore.Test.Unit.Requests
{
	public partial class ReportPerformerTest
	{
		[Trait("Feature", "PS - Project Summary")]
		[Theory(DisplayName = "[UNIT][PSQ-001] - Project Key is Empty")]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("    ")]
		public async Task GetProjectSummaryQuery_ValidateAsync_ProjectKeyIsEmpty(string? key)
		{
			// Arrange
			var sut = new GetProjectSummaryQueryValidator();

			// Act
			var result = await sut.ValidateAsync(new GetProjectSummaryFaker().Key(key).Generate());

			// Assert
			Assert.False(result.IsValid);
		}

		[Trait("Feature", "PS - Project Summary")]
		[Fact(DisplayName = "[UNIT][PSQ-002] - Get Project Summary")]
		public async Task GetProjectSummaryQuery_PerformAsync_GetProjectSummary()
		{
			// Arrange
			var sut = CreateSUT();
			var query = new GetProjectSummaryFaker().Generate();
			var project = new ProjectFaker().Key(query.Key).Generate();

			await _database.InsertAsync(project);

			// Act
			var result = await sut.PerformAsync(query, default);

			// Assert
			var version = project.Versions.OrderBy(v => v.UploadedAt).Last();
			var report = version.Reports.OrderBy(r => r.UploadedAt).Last();
			Assert.Equal(project.Key, result.Key);
			Assert.Equal(version.Version, result.Version);
			Assert.Equal(report.UploadedAt, result.LastReport);
			Assert.Equal(report.Features.Sum(f => f.PassCount), result.PassCount);
			Assert.Equal(report.Features.Sum(f => f.FailCount), result.FailCount);
			Assert.Equal(report.Features.Sum(f => f.SkippedCount), result.SkippedCount);
		}

		[Trait("Feature", "PS - Project Summary")]
		[Fact(DisplayName = "[UNIT][PSQ-003] - Project is not found")]
		public async Task GetProjectSummaryQuery_PerformAsync_ProjectIsNotFound()
		{
			// Arrange
			var sut = CreateSUT();
			var query = new GetProjectSummaryFaker().Generate();

			// Act
			// Assert
			await Assert.ThrowsAsync<NotFoundException>(async () => await sut.PerformAsync(query, default));
		}
	}

	file static class ReportPerformerTestExtensions
	{
		public static async Task InsertAsync(this DbContextOptions<ReportContext> options, ProjectEntity entity)
		{
			using var context = new ReportContext(options, new FakeClock());
			using var transaction = await context.Database.BeginTransactionAsync();

			await context.Projects.AddAsync(entity);

			await context.SaveChangesAsync();
			await transaction.CommitAsync();
		}
	}
}
