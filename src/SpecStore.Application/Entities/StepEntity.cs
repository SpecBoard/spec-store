namespace SpecStore.Application.Entities
{
	public class StepEntity
	{
		public int Id { get; set; }
		public StepType Type { get; set; }
		public required string Text { get; set; }
		public required Status Status { get; set; }
		public TimeSpan Duration { get; set; }
	}
}
