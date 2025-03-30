using AutoBogus;

namespace SpecStore.Test.Unit.Fakers
{
	public class GetProjectEvolutionFaker : AutoFaker<GetProjectEvolutionQuery>
	{
		public GetProjectEvolutionFaker Key(string? key)
		{
			CustomInstantiator(_ => new GetProjectEvolutionQuery(key!));

			return this;
		}
	}
}
