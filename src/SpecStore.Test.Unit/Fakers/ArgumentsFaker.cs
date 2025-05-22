using Bogus;

namespace SpecStore.Test.Unit.Fakers
{
	public class ArgumentsFaker
	{
		private readonly Faker _faker = new();
		private readonly List<Func<string[]>> _rules = new();

		public ArgumentsFaker()
		{
			_rules.Add(() => [_faker.Random.String()]);
		}

		public string[] Generate()
		{
			return [.. _rules.SelectMany(r => r())];
		}
	}
}
