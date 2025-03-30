using FluentValidation;
using STrain;

namespace SpecStore
{
	public record GetProjectEvolutionQuery : Query<IEnumerable<GetProjectEvolutionQuery.Result>>
	{
		public string Key { get; }

		public GetProjectEvolutionQuery(string key)
		{
			Key = key;
		}

		public record Result
		{
			public required int Id { get; init; }
			public required string Version { get; init; }
			public required int Pass { get; init; }
			public required int Fail { get; init; }
			public required int Skipped { get; init; }
		}
	}
}

namespace SpecStore.Api
{
	public class GetProjectEvolutionQueryValidator : AbstractValidator<GetProjectEvolutionQuery>
	{
		public GetProjectEvolutionQueryValidator()
		{
			RuleFor(q => q.Key).NotEmpty();
		}
	}
}
