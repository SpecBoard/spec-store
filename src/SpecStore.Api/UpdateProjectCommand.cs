using FluentValidation;
using STrain;

namespace SpecStore
{
	public record UpdateProjectCommand : Command
	{
		public required string Key { get; init; }
		public string? Name { get; init; }
	}
}

namespace SpecStore.Api
{
	public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
	{
		public UpdateProjectCommandValidator()
		{
			RuleFor(c => c.Key).NotEmpty();
		}
	}
}
