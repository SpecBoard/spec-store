using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using STrain;
using STrain.Core.Exceptions;

namespace SpecStore.Application.Performers
{
	public class ReportPerformers : IQueryPerformer<GetProjectsQuery, IEnumerable<GetProjectsQuery.Result>>,
		IQueryPerformer<GetProjectSummaryQuery, GetProjectSummaryQuery.Result>,
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

			var projects = await _context.Projects
									.Include(p => p.Versions)
										.ThenInclude(v => v.Reports)
											.ThenInclude(r => r.Features)
												.ThenInclude(f => f.Rules)
													.ThenInclude(r => r.Scenarios)
														.ThenInclude(s => s.Steps)
									.Include(p => p.Versions)
										.ThenInclude(v => v.Reports)
											.ThenInclude(r => r.Features)
												.ThenInclude(f => f.Scenarios)
													.ThenInclude(s => s.Steps)
									.OrderBy(p => p.Key).ToListAsync(cancellationToken);

			_logger.LogTrace("Projects: {@Project}", projects);

			_logger.LogInformation("Queried {ProjectCount} projects", projects.Count);
			return projects.Select(p => p.AsResult()).ToList();
		}

		public async Task<GetProjectSummaryQuery.Result> PerformAsync(GetProjectSummaryQuery query, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Querying summary of {Project} project", query.Key);

			var project = await _context.Projects
									.Include(p => p.Versions)
										.ThenInclude(v => v.Reports)
											.ThenInclude(r => r.Features)
												.ThenInclude(f => f.Rules)
													.ThenInclude(r => r.Scenarios)
														.ThenInclude(s => s.Steps)
									.Include(p => p.Versions)
										.ThenInclude(v => v.Reports)
											.ThenInclude(r => r.Features)
												.ThenInclude(f => f.Scenarios)
													.ThenInclude(s => s.Steps)
									.SingleOrDefaultAsync(p => p.Key == query.Key, cancellationToken);

			if (project is null)
			{
				_logger.LogError("{Project} project was not found", query.Key);
				throw new NotFoundException(query.Key);
			}

			var version = project.Versions.OrderBy(v => v.UploadedAt).Last();
			var report = version.Reports.OrderBy(v => v.UploadedAt).Last();

			_logger.LogInformation("Queried summary of {Project} project", query.Key);

			var result = new GetProjectSummaryQuery.Result
			{
				Key = project.Key,
				Version = version.Version,
				LastReport = report.UploadedAt,
				PassCount = report.Features.Sum(f => f.PassCount),
				FailCount = report.Features.Sum(f => f.FailCount),
				SkippedCount = report.Features.Sum(f => f.SkippedCount)
			};
			_logger.LogDebug("Result: {@Project}", result);

			return result;
		}

		public async Task PerformAsync(UploadReportCommand command, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Uploading report for '{Project}'", command.Project);

			await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

			var project = await _context.Projects
									.Include(p => p.Versions)
									.FirstOrDefaultAsync(p => p.Key == command.Project, cancellationToken: cancellationToken);
			if (project is null)
			{
				project = new ProjectEntity { Key = command.Project };
				await _context.Projects.AddAsync(project, cancellationToken).ConfigureAwait(false);
			}

			var version = project.Versions.SingleOrDefault(v => v.Version == command.Version);
			if (version is null)
			{
				version = new VersionEntity { Project = project, Version = command.Version };
				await _context.Versions.AddAsync(version, cancellationToken).ConfigureAwait(false);
			}

			version.Reports.Add(new ReportEntity
			{
				Metadata = command.Metadata.AsEntity(),
				Features = command.Features.AsEntity()
			});

			await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

			_logger.LogInformation("Report to '{Project}' project has been uploaded", command.Project);
		}
	}

	file static class ReportPerformersExtensions
	{
		public static ICollection<MetadataEntity> AsEntity(this IDictionary<string, string> metadata)
		{
			return [.. metadata.Select(m => new MetadataEntity { Key = m.Key, Value = m.Value })];
		}

		public static ICollection<FeatureEntity> AsEntity(this IEnumerable<UploadReportCommand.Feature> features)
		{
			return [.. features.Select(f => new FeatureEntity {
				Title = f.Title,
				Tags = f.Tags.AsEntity(),
				Rules = f.Rules.AsEntity(),
				Scenarios = f.Scenarios.AsEntity()
			})];
		}

		public static ICollection<TagEntity> AsEntity(this IEnumerable<string> tags)
		{
			return [.. tags.Select(t => new TagEntity { Tag = t })];
		}

		public static ICollection<RuleEntity> AsEntity(this IEnumerable<UploadReportCommand.Rule> rules)
		{
			return [.. rules.Select(r => new RuleEntity
			{
				Title = r.Title,
				Description = r.Description,
				Scenarios = r.Scenarios.AsEntity()
			})];
		}

		public static ICollection<ScenarioEntity> AsEntity(this IEnumerable<UploadReportCommand.Scenario> scenarios)
		{
			return [.. scenarios.Select(s => new ScenarioEntity
			{
				Title = s.Title,
				Tags = s.Tags.AsEntity(),
				Steps = s.Steps.AsEntity()
			})];
		}

		public static ICollection<StepEntity> AsEntity(this IEnumerable<UploadReportCommand.Step> steps)
		{
			return [.. steps.Select(s => new StepEntity
			{
				Text = s.Text,
				Status = s.Status,
				Type = s.Type,
				Duration = s.Duration
			})];
		}

		public static GetProjectsQuery.Result AsResult(this ProjectEntity project)
		{
			var version = project.Versions.OrderBy(v => v.UploadedAt).Last();
			var report = version.Reports.OrderBy(r => r.UploadedAt).Last();

			return new GetProjectsQuery.Result
			{
				Key = project.Key,
				Version = version.Version,
				LastReport = report.UploadedAt,
				PassCount = report.Features.Sum(f => f.PassCount),
				FailCount = report.Features.Sum(f => f.FailCount),
				SkippedCount = report.Features.Sum(f => f.SkippedCount)
			};
		}
	}
}
