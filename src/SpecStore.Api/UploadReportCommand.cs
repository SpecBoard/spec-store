using FluentValidation;
using SpecStore;
using STrain;

namespace SpecStore
{
	public record UploadReportCommand : Command
	{
		public required string Project { get; init; }
		public required string Version { get; init; }
	}

}

namespace Spector.Api
{
	public class UploadReportCommandValidator : AbstractValidator<UploadReportCommand>
	{
		public UploadReportCommandValidator()
		{
			RuleFor(c => c.Project).NotEmpty();
			RuleFor(c => c.Version).NotEmpty();
		}
	}

}
