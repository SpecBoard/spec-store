

namespace SpecStore.Application.Entities
{
	public class ProjectEntity : ITrackedEntity
	{
		public required string Key { get; set; }
		public ICollection<VersionEntity> Versions { get; set; } = [];
		public DateTime UploadedAt { get; set; }
	}
}
