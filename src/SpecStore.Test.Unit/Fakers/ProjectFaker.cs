using AutoBogus;
using Bogus.Extensions;
using SpecStore.Application.Entities;

namespace SpecStore.Test.Unit.Fakers
{
	public class ProjectFaker : AutoFaker<ProjectEntity>
	{
		public ProjectFaker()
		{
			AutoFaker.Configure(builder => builder.WithOverride(context => context.Faker.Date.Timespan(TimeSpan.FromMinutes(10))));

			var versionFaker = new AutoFaker<VersionEntity>();
			RuleFor(p => p.Versions, (_, p) => versionFaker.RuleFor(v => v.Project, p).GenerateBetween(1, 3));
		}

		public ProjectFaker Key(string key)
		{
			RuleFor(p => p.Key, key);

			return this;
		}
	}
}
