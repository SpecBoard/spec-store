namespace SpecStore.Application.Entities
{
	public class RuleEntity
	{
		public int Id { get; set; }
		public required string Title { get; set; }
		public string? Description { get; set; }
		public ICollection<ScenarioEntity> Scenarios { get; set; } = [];
	}
}
