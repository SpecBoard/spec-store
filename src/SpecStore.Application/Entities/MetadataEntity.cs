namespace SpecStore.Application.Entities
{
	public class MetadataEntity
	{
		public int Id { get; set; }
		public required string Key { get; set; }
		public required string Value { get; set; }
	}
}
