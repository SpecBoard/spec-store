using AutoBogus;

namespace SpecStore.Test.Unit.Fakers
{
	public class GetProjectSummaryFaker : AutoFaker<GetProjectSummaryQuery>
	{
		public GetProjectSummaryFaker Key(string? key)
		{
			RuleFor(q => q.Key, key);

			return this;
		}
	}
}
