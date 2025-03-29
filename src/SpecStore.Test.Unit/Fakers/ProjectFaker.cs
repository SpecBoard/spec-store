using AutoBogus;
using Bogus.Extensions;
using SpecStore.Application.Entities;

namespace SpecStore.Test.Unit.Fakers
{
	public class ProjectFaker : AutoFaker<ProjectEntity>
	{
		public ProjectFaker()
		{
			RuleFor(p => p.Versions, (_, p) => new AutoFaker<VersionEntity>().RuleFor(v => v.Project, p).GenerateBetween(1, 3));
		}

		public ProjectFaker Key(string key)
		{
			RuleFor(p => p.Key, key);

			return this;
		}
	}
}
