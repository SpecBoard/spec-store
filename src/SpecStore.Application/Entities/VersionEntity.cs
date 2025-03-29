
namespace SpecStore.Application.Entities
{
    public class VersionEntity : ITrackedEntity
    {
        public int Id { get; set; }
        public required ProjectEntity Project { get; set; }
        public required string Version { get; set; }
        public ICollection<ReportEntity> Reports { get; set; } = [];

        public DateTimeOffset UploadedAt { get; set; }
    }
}
