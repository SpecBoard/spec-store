using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SpecStore.Application.Converters
{
	public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
	{
		public UtcDateTimeConverter()
			: base(value => value.ToUniversalTime(),
				  value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
		{

		}
	}
}
