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
			Assert.Equal(report.Features.Sum(f => f.PassCount), result.Pass);
			Assert.Equal(report.Features.Sum(f => f.FailCount), result.Fail);
			Assert.Equal(report.Features.Sum(f => f.SkippedCount), result.Skipped);
			Assert.Equal(report.Features.CalculateDuration(), result.Duration);
			Assert.Collection(result.FailedScenarios.OrderBy(s => s.Id), [.. report.Features.GetFailedScenarios().OrderBy(s => s.Id).Inspect()]);
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

		public static IEnumerable<GetProjectSummaryQuery.Result.ScenarioSummary> GetFailedScenarios(this IEnumerable<FeatureEntity> features)
		{
			var result = new List<GetProjectSummaryQuery.Result.ScenarioSummary>();

			foreach (var feature in features.Where(f => f.FailCount > 0))
			{
				foreach (var rule in feature.Rules.Where(r => r.FailCount > 0))
				{
					foreach (var scenario in rule.Scenarios.Where(s => s.Status == Status.Fail))
					{
						result.Add(new GetProjectSummaryQuery.Result.ScenarioSummary { Id = scenario.Id, Segments = [feature.Title, rule.Title, scenario.Title] });
					}
				}

				foreach (var scenario in feature.Scenarios.Where(s => s.Status == Status.Fail))
				{
					result.Add(new GetProjectSummaryQuery.Result.ScenarioSummary { Id = scenario.Id, Segments = [feature.Title, scenario.Title] });
				}
			}

			return result;
		}

		public static IEnumerable<Action<GetProjectSummaryQuery.Result.ScenarioSummary>> Inspect(this IEnumerable<GetProjectSummaryQuery.Result.ScenarioSummary> scenarios)
		{
			foreach (var scenario in scenarios)
			{
				yield return s => Assert.Collection(s.Segments, [.. scenario.Segments.Inspect()]);
			}
		}

		public static IEnumerable<Action<string>> Inspect(this IEnumerable<string> values)
		{
			foreach (var value in values)
			{
				yield return v => Assert.Equal(value, v);
			}
		}

		public static TimeSpan CalculateDuration(this IEnumerable<FeatureEntity> features)
		{
			var result = TimeSpan.Zero;
			foreach (var step in features.SelectMany(f => f.Rules.SelectMany(r => r.Scenarios.SelectMany(s => s.Steps))).Concat(features.SelectMany(f => f.Scenarios.SelectMany(s => s.Steps))))
			{
				result += step.Duration;
			}
			return result;
		}
	}
}
