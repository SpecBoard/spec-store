using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using STrain;
using STrain.Core.Exceptions;
using STrain.Eventing.Publishers;

namespace SpecStore.Application.Performers
{
	public class ReportPerformers : IQueryPerformer<GetProjectsQuery, IEnumerable<GetProjectsQuery.Result>>,
		IQueryPerformer<GetProjectSummaryQuery, GetProjectSummaryQuery.Result>,
		IQueryPerformer<GetProjectEvolutionQuery, IEnumerable<GetProjectEvolutionQuery.Result>>,
		ICommandPerformer<UploadReportCommand>,
		ICommandPerformer<UpdateProjectCommand>

	{
		private readonly ReportContext _context;
		private readonly IPublisher _publisher;
		private readonly ILogger<ReportPerformers> _logger;

		public ReportPerformers(ReportContext context, IPublisher publisher, ILogger<ReportPerformers> logger)
		{
			_context = context;
			_publisher = publisher;
			_logger = logger;
		}

		public async Task<IEnumerable<GetProjectsQuery.Result>> PerformAsync(GetProjectsQuery query, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Quering projects");

			var projects = await _context.Projects
									.AsNoTracking()
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
													.ThenInclude(s => s.Steps).ToListAsync(cancellationToken);

			_logger.LogTrace("Projects: {@Project}", projects);

			_logger.LogInformation("Queried {ProjectCount} projects", projects.Count);
			return [.. projects.Select(p => p.AsResult()).OrderByDescending(p => p.LastReport)];
		}

		public async Task<GetProjectSummaryQuery.Result> PerformAsync(GetProjectSummaryQuery query, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Querying summary of {Project} project", query.Key);

			var project = await _context.Projects
									.AsNoTracking()
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
				throw new NotFoundException("/errors/resource-not-found", "Not Found", $"Project with '{query.Key}' key was not found");
			}

			var version = project.Versions.OrderBy(v => v.UploadedAt).Last();
			var report = version.Reports.OrderBy(v => v.UploadedAt).Last();

			_logger.LogInformation("Queried summary of {Project} project", query.Key);

			var result = new GetProjectSummaryQuery.Result
			{
				Key = project.Key,
				Name = project.Name,
				Version = version.Version,
				LastReport = report.UploadedAt,
				Pass = report.Features.Sum(f => f.PassCount),
				Fail = report.Features.Sum(f => f.FailCount),
				Skipped = report.Features.Sum(f => f.SkippedCount),
				Duration = report.Duration,
				FailedScenarios = [.. report.Features.GetFailedScenarios()]
			};
			_logger.LogDebug("Result: {@Project}", result);

			return result;
		}

		public async Task<IEnumerable<GetProjectEvolutionQuery.Result>> PerformAsync(GetProjectEvolutionQuery query, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Query evolution of {Project} project", query.Key);
			var reports = await _context.Reports
											.AsNoTracking()
											.Include(r => r.Version)
											.Include(r => r.Features)
												.ThenInclude(f => f.Rules)
													.ThenInclude(r => r.Scenarios)
														.ThenInclude(s => s.Steps)
											.Include(r => r.Features)
												.ThenInclude(f => f.Scenarios)
													.ThenInclude(s => s.Steps)
											.OrderByDescending(r => r.UploadedAt)
											.Where(r => r.Version.Project.Key == query.Key)
											.Take(4)
											.ToListAsync(cancellationToken: cancellationToken);

			_logger.LogInformation("Queried evolution of {Project} project", query.Key);

			var result = reports.ConvertAll(r => new GetProjectEvolutionQuery.Result
			{
				Id = r.Id,
				Version = r.Version.Version,
				Pass = r.Features.Sum(f => f.PassCount),
				Fail = r.Features.Sum(f => f.FailCount),
				Skipped = r.Features.Sum(f => f.SkippedCount)
			});

			_logger.LogTrace("Result: {@Evolution}", result);

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

			var report = new ReportEntity
			{
				Metadata = command.Metadata.AsEntity(),
				Features = command.Features.AsEntity()
			};

			version.Reports.Add(report);

			await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

			await _publisher.PublishAsync(new ReportUploadedEvent { Project = command.Project, Version = command.Version, Status = report.GetStatus() }, "specstore.report.uploaded", cancellationToken).ConfigureAwait(false);
			_logger.LogInformation("Report to '{Project}' project has been uploaded", command.Project);
		}

		public async Task PerformAsync(UpdateProjectCommand command, CancellationToken cancellationToken)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
			var project = await _context.Projects.FindAsync([command.Key], cancellationToken: cancellationToken);

			if (project is null) throw new NotFoundException($"Project was not found with key: {command.Key}");

			project.Name = command.Name;

			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
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
				Name = project.Name,
				Version = version.Version,
				LastReport = report.UploadedAt,
				PassCount = report.Features.Sum(f => f.PassCount),
				FailCount = report.Features.Sum(f => f.FailCount),
				SkippedCount = report.Features.Sum(f => f.SkippedCount)
			};
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
		public static Status GetStatus(this ReportEntity entity)
		{
			if (entity.Features.Any(f => f.FailCount > 0)) return Status.Fail;
			if (entity.Features.Any(f => f.SkippedCount > 0)) return Status.Skipped;
			return Status.Pass;
		}
	}

}
