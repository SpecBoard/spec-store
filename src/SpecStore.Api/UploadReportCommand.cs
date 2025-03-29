using FluentValidation;
using SpecStore;
using STrain;

namespace SpecStore
{
	public enum Status
	{
		Skipped,
		Fail,
		Pass
	}

	public enum StepType
	{
		Given,
		When,
		Then
	}

	public record UploadReportCommand : Command
	{
		public required string Project { get; init; }
		public required string Version { get; init; }
		public IDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

		public required IEnumerable<Feature> Features { get; init; } = [];

		public record Feature
		{
			public IEnumerable<string> Tags { get; init; } = [];
			public required string Title { get; init; }
			public string? Description { get; init; }
			public IEnumerable<Rule> Rules { get; init; } = [];
			public IEnumerable<Scenario> Scenarios { get; init; } = [];

		}

		public record Rule
		{
			public required string Title { get; init; }
			public string? Description { get; init; }
			public IEnumerable<Scenario> Scenarios { get; init; } = [];
		}

		public record Scenario
		{
			public IEnumerable<string> Tags { get; init; } = [];
			public required string Title { get; init; }
			public required IEnumerable<Step> Steps { get; init; } = [];
		}

		public record Step
		{
			public required StepType Type { get; init; }
			public required string Text { get; init; }
			public required Status Status { get; init; }
			public required TimeSpan Duration { get; init; }
		}
	}
}

namespace Spector.Api
{
	public class UploadReportCommandValidator : AbstractValidator<UploadReportCommand>
	{
		public UploadReportCommandValidator()
		{
			RuleFor(c => c.Project).NotEmpty();
			RuleFor(c => c.Version).NotEmpty();
		}
	}

}
