using FluentValidation;
using STrain;

namespace SpecStore
{
	public record GetProjectSummaryQuery : Query<GetProjectSummaryQuery.Result>
	{
		public string Key { get; }

		public GetProjectSummaryQuery(string key)
		{
			Key = key;
		}

		public record Result
		{
			public required string Key { get; init; }
			public required string Version { get; init; }
			public required DateTimeOffset LastReport { get; init; }
			public required int Pass { get; set; }
			public required int Fail { get; set; }
			public required int Skipped { get; init; }
			public required TimeSpan Duration { get; init; }
			public IEnumerable<ScenarioSummary> FailedScenarios { get; init; } = [];

			public record ScenarioSummary
			{
				public required int Id { get; set; }
				public IEnumerable<string> Segments { get; init; } = [];
			}
		}
	}
}

namespace SpecStore.Api
{
	public class GetProjectSummaryQueryValidator : AbstractValidator<GetProjectSummaryQuery>
	{
		public GetProjectSummaryQueryValidator()
		{
			RuleFor(q => q.Key).NotEmpty();
		}
	}
}
