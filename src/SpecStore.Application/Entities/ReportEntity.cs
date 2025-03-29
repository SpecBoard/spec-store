namespace SpecStore.Application.Entities
{
	public class ReportEntity : ITrackedEntity
	{
		public int Id { get; set; }
		public ICollection<MetadataEntity> Metadata { get; set; } = [];
		public ICollection<FeatureEntity> Features { get; set; } = [];

		public DateTimeOffset UploadedAt { get; set; }
	}
}
