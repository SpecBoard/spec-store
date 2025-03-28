namespace SpecStore.Application.Entities
{
	public class ProjectEntity
	{
		public required string Key { get; set; }
		public ICollection<VersionEntity> Versions { get; set; } = [];
	}
}
