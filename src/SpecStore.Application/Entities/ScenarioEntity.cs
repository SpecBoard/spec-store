namespace SpecStore.Application.Entities
{
	public class ScenarioEntity
	{
		public int Id { get; set; }
		public ICollection<TagEntity> Tags { get; set; } = [];
		public required string Title { get; set; }
		public ICollection<StepEntity> Steps { get; set; } = [];
	}
}
