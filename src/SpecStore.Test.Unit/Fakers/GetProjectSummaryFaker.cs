using AutoBogus;

namespace SpecStore.Test.Unit.Fakers
{
	public class GetProjectSummaryFaker : AutoFaker<GetProjectSummaryQuery>
	{
		public GetProjectSummaryFaker Key(string? key)
		{
			CustomInstantiator(_ => new GetProjectSummaryQuery(key!));

			return this;
		}
	}
}
