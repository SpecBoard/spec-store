namespace SpecStore.Application.Entities
{
	public interface ITrackedEntity
	{
		public DateTimeOffset UploadedAt { get; set; }
	}
}
