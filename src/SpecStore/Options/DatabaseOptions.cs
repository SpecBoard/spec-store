using System.ComponentModel.DataAnnotations;

namespace SpecStore.Options
{
	public record DatabaseOptions
	{
		[Required]
		public required string ConnectionString { get; init; }
	}
}
