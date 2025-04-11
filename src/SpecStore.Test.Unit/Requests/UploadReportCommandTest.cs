using Microsoft.EntityFrameworkCore;
using Moq;
using SpecStore.Api;
using SpecStore.Application.Contexts;
using SpecStore.Application.Entities;
using SpecStore.Test.Unit.Fakers;
using Spector.Api;

namespace SpecStore.Test.Unit.Requests
{
	[Trait("Feature", "MP - Managing Projects")]
	public partial class ReportPerformerTest
	{
		[Trait("Feature", "MP - Managing Projects")]
		[Theory(DisplayName = "[UNIT][UPR-001]: Project is not defined")]
		[InlineData("")]
		[InlineData(null)]
		[InlineData("   ")]
		public async Task UploadReportCommand_ValidateAsync_ProjectIsNull(string? project)
		{
			// Arrange
			var sut = new UploadReportCommandValidator();

			// Act
			var result = await sut.ValidateAsync(new UploadReportFaker().Project(project).Generate(), default);

			// Assert
			Assert.False(result.IsValid);
		}

		[Trait("Feature", "MP - Managing Projects")]
		[Theory(DisplayName = "[UNIT][UPR-002]: Version is not defined")]
		[InlineData("")]
		[InlineData(null)]
		[InlineData("   ")]
		public async Task UploadReportCommand_ValidateAsync_VersionIsNull(string? project)
		{
			// Arrange
			var sut = new UploadReportCommandValidator();

			// Act
			var result = await sut.ValidateAsync(new UploadReportFaker().Version(project).Generate(), default);

			// Assert
			Assert.False(result.IsValid);
		}

		[Trait("Feature", "MP - Managing Projects")]
		[Fact(DisplayName = "[UNIT][UPR-003]: Upload Report")]
		public async Task UploadReportCommand_PerformAsyn_UploadReport()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			// Act
			await sut.PerformAsync(command, default);

			// Assert
			Assert.Collection(await _database.GetProjectsAsync(), p => p.Verify(command));
		}

		[Trait("Feature", "MP - Managing Projects")]
		[Fact(DisplayName = "[UNIT][UPR-004]: Upload Report to Existing Project")]
		public async Task UploadReportCommand_PerformAsyn_UploadReportToExistingProject()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			await _database.InsertAsync(command.Project, command.Version);

			// Act
			// Assert
			await sut.PerformAsync(command, default);
		}

		[Trait("Feature", "MP - Managing Projects")]
		[Fact(DisplayName = "[UNIT][UPR-005]: Upload Report to Existing Version")]
		public async Task UploadReportCommand_PerformAsyn_UploadReportToExistingVersion()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			await _database.InsertAsync(command.Project, command.Version);

			// Act
			await sut.PerformAsync(command, default);

			// Assert
			Assert.Collection(await _database.GetVersionsAsync(command.Project), p => p.Version = command.Version);
		}

		[Trait("Feature", "MP - Managing Projects")]
		[Fact(DisplayName = "[UNIT][UPR-006]: Publish Event about Uploaded Report")]
		public async Task UploadReportCommand_PerformAsyn_PublishEventAboutUploadedReport()
		{
			// Arrange
			var sut = CreateSUT();
			var command = new UploadReportFaker().Generate();

			// Act
			await sut.PerformAsync(command, default);

			// Assert
			_publisherMock.Verify(p => p.PublishAsync(It.Is<ReportUploadedEvent>(e => e.Project == command.Project && e.Version == command.Version), "specstore.report.uploaded", It.IsAny<CancellationToken>()), Times.Once());
		}
	}

	file static class ReportPerformerTestExtensions
	{
		public static async Task<IEnumerable<ProjectEntity>> GetProjectsAsync(this DbContextOptions<ReportContext> database)
		{
			using var context = new ReportContext(database, new FakeClock());
			return await context.Projects.ToListAsync();
		}

		public static async Task<IEnumerable<VersionEntity>> GetVersionsAsync(this DbContextOptions<ReportContext> database, string project)
		{
			using var context = new ReportContext(database, new FakeClock());
			return [.. (await context.Projects.Include(p => p.Versions).FirstAsync(e => e.Key == project))!.Versions];
		}

		public static async Task InsertAsync(this DbContextOptions<ReportContext> database, string project, string version)
		{
			await using var context = new ReportContext(database, new FakeClock());
			await using var transaction = await context.Database.BeginTransactionAsync();
			var entity = new ProjectEntity { Key = project };
			await context.AddAsync(entity);
			await context.AddAsync(new VersionEntity { Project = entity, Version = version });
			await context.SaveChangesAsync();
			await transaction.CommitAsync();
		}

		public static bool Verify(this ProjectEntity entity, UploadReportCommand command)
		{
			return entity.Key == command.Project
				&& entity.Versions.Verify(v =>
				{
					return v.Version == command.Version
						&& v.Reports.Verify(r => r.Verify(command));
				});
		}

		public static bool Verify<T>(this IEnumerable<T> collection, params Func<T, bool>[] inspector)
		{
			return collection.Count() == inspector.Length &&
				inspector.ToList().TrueForAll(i => collection.Any(c => i(c)));
		}

		public static bool Verify(this ReportEntity report, UploadReportCommand command)
		{
			return report.Metadata.Verify([.. command.Metadata.Inspect()])
				&& report.Features.Verify([.. command.Features.Inspect()]);
		}

		public static IEnumerable<Func<MetadataEntity, bool>> Inspect(this IDictionary<string, string> metadata)
		{
			foreach (var item in metadata)
			{
				yield return m => m.Key == item.Key && m.Value == item.Value;
			}
		}

		public static IEnumerable<Func<FeatureEntity, bool>> Inspect(this IEnumerable<UploadReportCommand.Feature> features)
		{
			foreach (var feature in features)
			{
				yield return f => f.Title == feature.Title &&
							f.Tags.Verify([.. feature.Tags.Inspect()]) &&
							f.Rules.Verify([.. feature.Rules.Inspect()]) &&
							f.Scenarios.Verify([.. feature.Scenarios.Inspect()]);
			}
		}

		public static IEnumerable<Func<TagEntity, bool>> Inspect(this IEnumerable<string> tags)
		{
			foreach (var tag in tags)
			{
				yield return f => f.Tag == tag;
			}
		}

		public static IEnumerable<Func<RuleEntity, bool>> Inspect(this IEnumerable<UploadReportCommand.Rule> rules)
		{
			foreach (var rule in rules)
			{
				yield return r => r.Title == rule.Title &&
					r.Description == rule.Description &&
					r.Scenarios.Verify([.. rule.Scenarios.Inspect()]);
			}
		}

		public static IEnumerable<Func<ScenarioEntity, bool>> Inspect(this IEnumerable<UploadReportCommand.Scenario> scenarios)
		{
			foreach (var scenario in scenarios)
			{
				yield return s => s.Tags.Verify([.. scenario.Tags.Inspect()]) &&
					s.Title == scenario.Title &&
					s.Steps.Verify([.. scenario.Steps.Inspect()]);
			}
		}

		public static IEnumerable<Func<StepEntity, bool>> Inspect(this IEnumerable<UploadReportCommand.Step> steps)
		{
			foreach (var step in steps)
			{
				yield return s => s.Type == step.Type &&
					s.Text == step.Text &&
					s.Duration == step.Duration &&
					s.Status == step.Status;
			}
		}
	}
}
