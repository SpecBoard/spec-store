namespace SpecStore.Application.Entities
{
	public class VersionEntity
	{
		public int Id { get; set; }
		public required ProjectEntity Project { get; set; }
		public required string Version { get; set; }
	}
}
