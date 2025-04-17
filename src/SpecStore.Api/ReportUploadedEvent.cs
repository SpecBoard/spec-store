using STrain;

namespace SpecStore
{
	public record ReportUploadedEvent : Event
	{
		public required string Project { get; init; }
		public required string Version { get; init; }
		public required Status Status { get; init; }
	}
}
