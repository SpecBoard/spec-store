using AutoBogus;

namespace SpecStore.Test.Unit.Fakers
{
	public class UpdateProjectFaker : AutoFaker<UpdateProjectCommand>
	{
		public UpdateProjectFaker Key(string? key)
		{
			RuleFor(p => p.Key, key);

			return this;
		}
	}
}
