using SpecStore.Api;
using SpecStore.Application.Entities;
using SpecStore.Test.Unit.Extensions;
using SpecStore.Test.Unit.Fakers;

namespace SpecStore.Test.Unit.Requests
{
	public partial class ReportPerformerTest
	{
		[Trait("Feature", "PE - Project Evolution")]
		[Theory(DisplayName = "[UNIT][PEQ-001]: Project is Empty")]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public async Task GetProjectEvolutionQuery_ValidateAsync_ProjectIsEmpty(string? key)
		{
			// Arrange
			var sut = new GetProjectEvolutionQueryValidator();

			// Act
			var result = await sut.ValidateAsync(new GetProjectEvolutionFaker().Key(key).Generate(), default);

			// Assert
			Assert.False(result.IsValid);
		}

		[Trait("Feature", "PE - Project Evolution")]
		[Fact(DisplayName = "[UNIT][PEQ-002]: Get Project Evolution")]
		public async Task GetProjectEvolutionQuery_PerformAsync_GetProjectEvolution()
		{
			// Arrange
			var sut = CreateSUT();
			var query = new GetProjectEvolutionFaker().Generate();
			var project = new ProjectFaker().Key(query.Key).Generate();

			await _database.InsertAsync(project);

			// Act
			var result = await sut.PerformAsync(query, default);

			// Assert
			Assert.Collection(result, [.. project.Versions.Inspect()]);
		}
	}

	file static class GetProjectEvolutionQueryExtensions
	{
		public static IEnumerable<Action<GetProjectEvolutionQuery.Result>> Inspect(this IEnumerable<VersionEntity> versions)
		{
			foreach (var report in versions.SelectMany(v => v.Reports).OrderBy(r => r.UploadedAt).Take(4))
			{
				yield return r =>
				{
					Assert.Equal(report.Id, r.Id);
					Assert.Equal(report.Version.Version, r.Version);
					Assert.Equal(report.Features.Sum(f => f.PassCount), r.Pass);
					Assert.Equal(report.Features.Sum(f => f.FailCount), r.Fail);
					Assert.Equal(report.Features.Sum(f => f.SkippedCount), r.Skipped);
				};
			}
		}
	}
}
