using STrain;

namespace SpecStore
{
	public record GetProjectsQuery : Query<IEnumerable<GetProjectsQuery.Result>>
	{
		public record Result
		{
			public required string Key { get; init; }
		}
	}
}
