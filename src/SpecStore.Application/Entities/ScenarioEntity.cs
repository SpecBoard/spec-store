using System.ComponentModel.DataAnnotations.Schema;

namespace SpecStore.Application.Entities
{
	public class ScenarioEntity
	{
		public int Id { get; set; }
		public ICollection<TagEntity> Tags { get; set; } = [];
		public required string Title { get; set; }
		public ICollection<StepEntity> Steps { get; set; } = [];

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public Status Status => Steps.Any(s => s.Status == Status.Fail) ? Status.Fail : Steps.Any(s => s.Status == Status.Skipped) ? Status.Skipped : Status.Pass;
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public TimeSpan Duration => TimeSpan.FromTicks(Steps.Sum(s => s.Duration.Ticks));
	}
}
