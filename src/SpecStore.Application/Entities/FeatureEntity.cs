namespace SpecStore.Application.Entities
{
	public class FeatureEntity
	{
		public int Id { get; set; }
		public ICollection<TagEntity> Tags { get; set; } = [];
		public required string Title { get; set; }
		public ICollection<RuleEntity> Rules { get; set; } = [];
		public ICollection<ScenarioEntity> Scenarios { get; set; } = [];
	}
}
