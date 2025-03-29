using Microsoft.AspNetCore.Mvc;
using STrain;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SpecStore.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProjectController : ControllerBase
	{
		private readonly IMvcRequestReceiver _requestReceiver;

		public ProjectController(IMvcRequestReceiver requestReceiver)
		{
			_requestReceiver = requestReceiver;
		}

		[HttpGet]
		public async Task<IActionResult> GetProjectsAsync(CancellationToken cancellationToken)
		{
			return await _requestReceiver.ReceiveQueryAsync(new GetProjectsQuery(), cancellationToken);
		}

		[HttpGet("{key}/summary")]
		public async Task<IActionResult> GetProjectSummaryAsync(string key, CancellationToken cancellationToken)
		{
			return await _requestReceiver.ReceiveQueryAsync(new GetProjectSummaryQuery { Key = key }, cancellationToken);
		}

		[HttpPost("{key}")]
		public async Task UploadReportAsync(string key, UploadReportBody body, CancellationToken cancellationToken)
		{
			await _requestReceiver.ReceiveCommandAsync(new UploadReportCommand
			{
				Project = key,
				Version = body.Version,
				Metadata = body.Metadata,
				Features = body.Features.AsCommand()
			}, cancellationToken);
		}
	}

	public record UploadReportBody
	{
		public required string Version { get; init; }
		public IDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
		public IEnumerable<FeatureBody> Features { get; init; } = [];
	}

	public record FeatureBody
	{
		public IEnumerable<string> Tags { get; init; } = [];
		public required string Title { get; init; }
		public string? Description { get; init; }
		public IEnumerable<RuleBody> Rules { get; init; } = [];
		public IEnumerable<ScenarioBody> Scenarios { get; init; } = [];
	}

	public record RuleBody
	{
		public required string Title { get; init; }
		public string? Description { get; init; }
		public IEnumerable<ScenarioBody> Scenarios { get; init; } = [];

	}

	public record ScenarioBody
	{
		public IEnumerable<string> Tags { get; init; } = [];
		public required string Title { get; init; }
		public IEnumerable<StepBody> Steps { get; init; } = [];
	}

	public record StepBody
	{
		public required string Text { get; init; }
		public required StepType Type { get; init; }
		public required TimeSpan Duration { get; init; }
		public required Status Status { get; init; }
	}

	file static class ProjectControllerExtensions
	{
		public static IEnumerable<UploadReportCommand.Feature> AsCommand(this IEnumerable<FeatureBody> features)
		{
			return [.. features.Select(f => new UploadReportCommand.Feature
			{
				Tags = f.Tags,
				Title = f.Title,
				Description = f.Description,
				Rules = f.Rules.AsCommand(),
				Scenarios = f.Scenarios.AsCommand()
			})];
		}

		public static IEnumerable<UploadReportCommand.Rule> AsCommand(this IEnumerable<RuleBody> rules)
		{
			return [.. rules.Select(r => new UploadReportCommand.Rule
			{
				Title = r.Title,
				Description = r.Description,
				Scenarios = r.Scenarios.AsCommand()
			})];
		}

		public static IEnumerable<UploadReportCommand.Scenario> AsCommand(this IEnumerable<ScenarioBody> scenarios)
		{
			return [.. scenarios.Select(s => new UploadReportCommand.Scenario
			{
				Title = s.Title,
				Tags = s.Tags,
				Steps = s.Steps.AsCommand()
			})];
		}

		public static IEnumerable<UploadReportCommand.Step> AsCommand(this IEnumerable<StepBody> steps)
		{
			return [.. steps.Select(s => new UploadReportCommand.Step
			{
				Text = s.Text,
				Type = s.Type,
				Status = s.Status,
				Duration = s.Duration,
			})];
		}
	}
}
