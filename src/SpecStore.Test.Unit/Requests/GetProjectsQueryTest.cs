using Bogus.Extensions;
using Microsoft.EntityFrameworkCore;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using SpecStore.Test.Unit.Extensions;
using SpecStore.Test.Unit.Fakers;

namespace SpecStore.Test.Unit.Requests
{
	public partial class ReportPerformerTest
	{
		[Trait("Feature", "MP - Managing Projects")]
		[Fact(DisplayName = "[UNIT][GPQ-001]: Get Projects")]
		public async Task GetProjectQuery_PerformAsync_GetProjects()
		{
			// Arrange
			var sut = CreateSUT();
			var projects = new ProjectFaker().GenerateBetween(2, 5);

			await _database.InsertAsync(projects);

			// Act
			var result = await sut.PerformAsync(new GetProjectsFaker().Generate(), default);

			// Assert
			Assert.All(result, r => projects.Any(p => p.Key == r.Key));
		}
	}

	file static class ReportPerformerTestExtensions
	{
		public static async Task<IEnumerable<GetProjectsQuery.Result>> GetProjectsAsync(this DbContextOptions<ReportContext> database)
		{
			await using var context = new ReportContext(database, new FakeClock());
			var projects = await context.Projects.Include(p => p.Versions).ToListAsync();

			return projects.ConvertAll(p => new GetProjectsQuery.Result
			{
				Key = p.Key,
				Version = p.Versions.OrderBy(v => v.UploadedAt).Last().Version
			});
		}

		public static IEnumerable<Action<GetProjectsQuery.Result>> Inspect(this IEnumerable<ProjectEntity> projects)
		{
			foreach (var project in projects.OrderBy(p => p.Key).ToList())
			{
				yield return r =>
				{
					var version = project.Versions.OrderBy(v => v.UploadedAt).Last();
					var report = version.Reports.OrderBy(r => r.UploadedAt).Last();

					Assert.Equal(r.Key, project.Key);
					Assert.Equal(r.Version, version.Version);
					Assert.Equal(r.LastReport, version.Reports.Max(r => r.UploadedAt));
					Assert.Equal(r.PassCount, report.Features.Sum(f => f.PassCount));
					Assert.Equal(r.FailCount, report.Features.Sum(f => f.FailCount));
				};
			}
		}
	}
}
