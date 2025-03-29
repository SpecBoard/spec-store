using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SpecStore.Application.Entities;

namespace SpecStore.Application.Interceptors
{
	public class TrackInterceptor : SaveChangesInterceptor
	{
		public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
		{
			if (eventData.Context is null) return ValueTask.FromResult(result);

			foreach (var entity in eventData.Context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged).Select(e => e.Entity).OfType<ITrackedEntity>())
			{
				entity.UploadedAt = DateTime.UtcNow;
			}

			return ValueTask.FromResult(result);
		}
	}
}
