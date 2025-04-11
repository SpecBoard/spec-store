using Microsoft.EntityFrameworkCore;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using SpecStore.Test.Unit.Fakers;

namespace SpecStore.Test.Unit.Extensions
{
	internal static class DatabaseContexExtensions
	{
		public static async Task InsertAsync(this DbContextOptions<ReportContext> options, IEnumerable<ProjectEntity> entities)
		{
			foreach (var entity in entities)
			{
				await options.InsertAsync(entity);
			}
		}

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
