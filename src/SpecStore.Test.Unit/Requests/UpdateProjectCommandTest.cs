using SpecStore.Api;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using SpecStore.Test.Unit.Extensions;
using SpecStore.Test.Unit.Fakers;
using STrain.Core.Exceptions;

namespace SpecStore.Test.Unit.Requests
{
	public partial class ReportPerformerTest
	{
		[Trait("Feature", "MP - Managing Projects")]
		[Theory(DisplayName = "[UNIT][UPC-001]: Key is empty")]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("	")]
		public async Task UpdateProjectCommand_ValidateAsync_KeyIsEmpty(string? key)
		{
			// Arrange
			var sut = new UpdateProjectCommandValidator();

			// Act
			var result = await sut.ValidateAsync(new UpdateProjectFaker().Key(key).Generate());

			// Assert
			Assert.False(result.IsValid);
		}

		[Trait("Feature", "MP - Managing Projects")]
		[Fact(DisplayName = "[UNIT][UPC-002]: Update Name")]
		public async Task UpdateProjectCommand_ValidateAsync_UpdateName()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UpdateProjectFaker().Generate();

			await _database.InsertAsync(command.AsEntity());

			// Act
			await sut.PerformAsync(command, default);

			// Assert
			await using var context = new ReportContext(_database, new FakeClock());
			Assert.Equal(command.Name, (await context.Projects.FindAsync(command.Key))!.Name);
		}

		[Trait("Feature", "MP - Managing Projects")]
		[Fact(DisplayName = "[UNIT][UPC-003]: Project does Not Found")]
		public async Task UpdateProjectCommand_ValidateAsync_ProjectDoesNotFound()
		{
			// Arrange
			var sut = CreateSUT();

			// Act
			// Assert
			await Assert.ThrowsAsync<NotFoundException>(async () => await sut.PerformAsync(new UpdateProjectFaker().Generate(), default));
		}
	}

	file static class ReportPerformerTestExtensions
	{
		public static ProjectEntity AsEntity(this UpdateProjectCommand command)
		{
			return new ProjectEntity { Key = command.Key };
		}
	}
}
