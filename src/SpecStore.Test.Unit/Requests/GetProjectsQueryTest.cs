using Bogus.Extensions;
using Microsoft.EntityFrameworkCore;
using SpecStore.Application.Contexts;
using SpecStore.Test.Unit.Fakers;

namespace SpecStore.Test.Unit.Requests
{
	public partial class ReportPerformerTest
	{
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
			Assert.Collection(await _database.GetProjectsAsync(), [.. projects.OrderBy(p => p.Key).Inspect()]);
		}
	}

	file static class ReportPerformerTestExtensions
	{
		public static async Task InsertAsync(this DbContextOptions<ReportContext> database, IEnumerable<GetProjectsQuery.Result> projects)
		{
			await using var context = new ReportContext(database);
			await using var transation = await context.Database.BeginTransactionAsync();

			await context.Projects.AddRangeAsync(projects.Select(q => new Application.Entities.ProjectEntity { Key = q.Key }).ToList());

			await context.SaveChangesAsync();
			await transation.CommitAsync();
		}

		public static async Task<IEnumerable<GetProjectsQuery.Result>> GetProjectsAsync(this DbContextOptions<ReportContext> database)
		{
			await using var context = new ReportContext(database);
			var projects = await context.Projects.ToListAsync();

			return projects.Select(p => new GetProjectsQuery.Result { Key = p.Key }).ToList();
		}

		public static IEnumerable<Action<GetProjectsQuery.Result>> Inspect(this IEnumerable<GetProjectsQuery.Result> projects)
		{
			foreach (var project in projects)
			{
				yield return r => Assert.Equal(r, project);
			}
		}
	}
}
