using System.ComponentModel.DataAnnotations.Schema;

namespace SpecStore.Application.Entities
{
	public class RuleEntity
	{
		public int Id { get; set; }
		public required string Title { get; set; }
		public string? Description { get; set; }
		public ICollection<ScenarioEntity> Scenarios { get; set; } = [];

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int PassCount => Scenarios.Count(s => s.Status == Status.Pass);

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int FailCount => Scenarios.Count(s => s.Status == Status.Fail);

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int SkippedCount => Scenarios.Count(s => s.Status == Status.Skipped);

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public TimeSpan Duration => TimeSpan.FromTicks(Scenarios.Sum(s => s.Duration.Ticks));
	}
}
