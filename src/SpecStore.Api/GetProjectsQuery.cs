using STrain;

namespace SpecStore
{
	public record GetProjectsQuery : Query<IEnumerable<GetProjectsQuery.Result>>
	{
		public record Result
		{
			public required string Key { get; init; }
			public required string Version { get; init; }
			public DateTimeOffset LastReport { get; init; }
			public int PassCount { get; init; }
			public int FailCount { get; init; }
			public int SkippedCount { get; init; }
		}
	}
}
