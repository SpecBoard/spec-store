using System.ComponentModel.DataAnnotations.Schema;

namespace SpecStore.Application.Entities
{
	public class FeatureEntity
	{
		public int Id { get; set; }
		public ICollection<TagEntity> Tags { get; set; } = [];
		public required string Title { get; set; }
		public ICollection<RuleEntity> Rules { get; set; } = [];
		public ICollection<ScenarioEntity> Scenarios { get; set; } = [];

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int PassCount => Rules.Sum(r => r.PassCount) + Scenarios.Count(s => s.Status == Status.Pass);
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int FailCount => Rules.Sum(r => r.FailCount) + Scenarios.Count(s => s.Status == Status.Fail);
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int SkippedCount => Rules.Sum(r => r.SkippedCount) + Scenarios.Count(s => s.Status == Status.Skipped);
	}
}
