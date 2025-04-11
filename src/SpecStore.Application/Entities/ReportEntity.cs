using System.ComponentModel.DataAnnotations.Schema;

namespace SpecStore.Application.Entities
{
	public class ReportEntity : ITrackedEntity
	{
		public int Id { get; set; }
		public VersionEntity Version { get; set; } = null!;
		public ICollection<MetadataEntity> Metadata { get; set; } = [];
		public ICollection<FeatureEntity> Features { get; set; } = [];

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public TimeSpan Duration => TimeSpan.FromTicks(Features.Sum(s => s.Duration.Ticks));

		public DateTimeOffset UploadedAt { get; set; }
	}
}
