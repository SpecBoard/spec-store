using Io.Cucumber.Messages.Types;

namespace SpecStore.Client.Cucumber.Readers
{
	public class EnvelopeReader : IReader
	{
		public IEnumerable<UploadReportCommand.Feature> ReadFeatures(IEnumerable<Envelope> envelope)
		{
			var result = new List<UploadReportCommand.Feature>();

			foreach (var testRun in envelope.Select(d => d.TestRunStarted).Where(trs => trs is not null))
			{
				foreach (var testCase in envelope.Select(d => d.TestCase).Where(tc => tc is not null && tc.TestRunStartedId == testRun.Id))
				{
					var pickle = envelope.Select(d => d.Pickle).Single(p => p is not null && p.Id == testCase.PickleId);
					var feature = envelope.Select(d => d.GherkinDocument).First(gd => gd is not null && gd.Feature.Children.Any(c => (c.Scenario is not null && c.Scenario.Id == pickle.AstNodeIds.First()) || (c.Rule is not null && c.Rule.Children.Any(c => c.Scenario.Id == pickle.AstNodeIds.First())))).Feature;

					result.Add(new UploadReportCommand.Feature
					{
						Title = feature.Name,
						Description = feature.Description,
						Tags = pickle.Tags.Select(t => t.Name),
						Rules = feature.Children.Select(r => r.Rule).ReadRules(pickle),
						Scenarios = pickle.ReadScenarios()
					});
				}
			}

			return result;
		}

		public IDictionary<string, string?> ReadMetadata(Meta meta)
		{
			var result = new Dictionary<string, string?>()
			{
				["OS_NAME"] = meta.Os.Name,
				["OS_VERSION"] = meta.Os.Version,

				["RUNTIME_NAME"] = meta.Runtime.Name,
				["RUNTIME_VERSION"] = meta.Runtime.Version,

				["CPU_NAME"] = meta.Cpu.Name,

				["IMPLEMENTATION_NAME"] = meta.Implementation.Name,
				["IMPLEMENTATION_VERSION"] = meta.Implementation.Version,

				["PROTOCOL_VERSION"] = meta.ProtocolVersion,
			};

			if (meta.Ci is not null)
			{
				if (!string.IsNullOrWhiteSpace(meta.Ci.Name)) result.Add("CI_NAME", meta.Ci.Name);
				if (!string.IsNullOrWhiteSpace(meta.Ci.BuildNumber)) result.Add("CI_BUILD_NUMBER", meta.Ci.BuildNumber);
				if (!string.IsNullOrWhiteSpace(meta.Ci.Url)) result.Add("CI_URL", meta.Ci.Url);

				if (!string.IsNullOrWhiteSpace(meta.Ci.Git.Remote)) result.Add("GIT_REMOTE", meta.Ci.Git.Remote);
				if (!string.IsNullOrWhiteSpace(meta.Ci.Git.Branch)) result.Add("GIT_BRANCH", meta.Ci.Git.Branch);
				if (!string.IsNullOrWhiteSpace(meta.Ci.Git.Revision)) result.Add("GIT_REVISION", meta.Ci.Git.Revision);
				if (!string.IsNullOrWhiteSpace(meta.Ci.Git.Tag)) result.Add("GIT_TAG", meta.Ci.Git.Tag);
			}

			if (!string.IsNullOrWhiteSpace(meta.Cpu.Version)) result.Add("CPU_VERSION", meta.Cpu.Version);

			return result;
		}
	}

	file static class EnvelopeReaderExtensions
	{
		public static IEnumerable<UploadReportCommand.Rule> ReadRules(this IEnumerable<Rule> rules, Pickle pickle)
		{
			var result = new List<UploadReportCommand.Rule>();
			foreach (var rule in rules)
			{
				result.Add(new UploadReportCommand.Rule
				{
					Title = rule.Name,
					Description = rule.Description,
					Scenarios = rule.Children.Where(c => c.Scenario is not null).Select(c => c.Scenario).ReadScenarios(pickle)
				});
			}
			return result;
		}

		public static IEnumerable<UploadReportCommand.Scenario> ReadScenarios(this Pickle pickle)
		{
			var result = new List<UploadReportCommand.Scenario>();
			foreach (var scenario in scenarios)
			{
				result.Add(new UploadReportCommand.Scenario
				{
					Title = scenario.Name,
					Tags = scenario.Tags.Select(t => t.Name),
					Steps = scenario.Steps.ReadSteps(),
				});
			}
			return result;
		}

		public static IEnumerable<UploadReportCommand.Step> ReadSteps(this IEnumerable<Step> steps)
		{
			var result = new List<UploadReportCommand.Step>();
			foreach (var given in steps.Where(s => s.KeywordType == StepKeywordType.CONTEXT))
			{
				result.Add(new UploadReportCommand.Step
				{
					Text = given.Text,
					Status = Status.Skipped,
					Duration = TimeSpan.Zero,
					Type = StepType.Given
				});
			}
			foreach (var given in steps.Where(s => s.KeywordType == StepKeywordType.ACTION))
			{
				result.Add(new UploadReportCommand.Step
				{
					Text = given.Text,
					Status = Status.Skipped,
					Duration = TimeSpan.Zero,
					Type = StepType.When
				});
			}
			foreach (var given in steps.Where(s => s.KeywordType == StepKeywordType.OUTCOME))
			{
				result.Add(new UploadReportCommand.Step
				{
					Text = given.Text,
					Status = Status.Skipped,
					Duration = TimeSpan.Zero,
					Type = StepType.Then
				});
			}
			return result;
		}
	}
}
