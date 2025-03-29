using Microsoft.Extensions.Internal;

namespace SpecStore.Test.Unit.Fakers
{
	public class FakeClock : ISystemClock
	{
		private int _counter = 0;

		public DateTimeOffset UtcNow => DateTimeOffset.FromUnixTimeSeconds(++_counter);
	}
}
