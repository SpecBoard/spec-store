using AutoBogus;

namespace SpecStore.Test.Unit.Fakers
{
	internal class UploadReportFaker : AutoFaker<UploadReportCommand>
	{
		public UploadReportFaker Project(string? project)
		{
			RuleFor(c => c.Project, project);

			return this;
		}

		public UploadReportFaker Version(string? version)
		{
			RuleFor(c => c.Version, version);

			return this;
		}
	}
}
