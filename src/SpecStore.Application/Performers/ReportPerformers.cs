using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using STrain;

namespace SpecStore.Application.Performers
{
	public class ReportPerformers : IQueryPerformer<GetProjectsQuery, IEnumerable<GetProjectsQuery.Result>>,
		ICommandPerformer<UploadReportCommand>
	{
		private readonly ReportContext _context;
		private readonly ILogger<ReportPerformers> _logger;

		public ReportPerformers(ReportContext context, ILogger<ReportPerformers> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<IEnumerable<GetProjectsQuery.Result>> PerformAsync(GetProjectsQuery query, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Quering projects");

			var projects = await _context.Projects.OrderBy(p => p.Key).ToListAsync(cancellationToken);

			_logger.LogTrace("Projects: {@Project}", projects);

			_logger.LogInformation("Queried {ProjectCount} projects", projects.Count);
			return projects.Select(p => new GetProjectsQuery.Result { Key = p.Key });
		}

		public async Task PerformAsync(UploadReportCommand command, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Uploading report for '{Project}'", command.Project);

			await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

			var project = await _context.Projects.Include(p => p.Versions).FirstOrDefaultAsync(p => p.Key == command.Project, cancellationToken: cancellationToken);
			if (project is null)
			{
				project = new ProjectEntity { Key = command.Project };
				await _context.Projects.AddAsync(project, cancellationToken).ConfigureAwait(false);
			}
			if (!project.Versions.Any(v => v.Version == command.Version)) await _context.Versions.AddAsync(new VersionEntity { Project = project, Version = command.Version });

			await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

			_logger.LogInformation("Report to '{Project}' project has been uploaded", command.Project);
		}
	}
}
