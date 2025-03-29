using FluentValidation;
using STrain;

namespace SpecStore
{
	public record GetProjectSummaryQuery : Query<GetProjectSummaryQuery.Result>
	{
		public required string Key { get; init; }

		public record Result
		{
			public required string Key { get; init; }
			public required string Version { get; init; }
			public required DateTimeOffset LastReport { get; init; }
			public required int PassCount { get; init; }
			public required int FailCount { get; init; }
			public required int SkippedCount { get; init; }
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
