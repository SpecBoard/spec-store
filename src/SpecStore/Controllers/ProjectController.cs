using Microsoft.AspNetCore.Mvc;
using STrain;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SpecStore.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProjectController : ControllerBase
	{
		private readonly IMvcRequestReceiver _requestReceiver;

		public ProjectController(IMvcRequestReceiver requestReceiver)
		{
			_requestReceiver = requestReceiver;
		}

		[HttpGet]
		public async Task<IActionResult> GetProjectsAsync(CancellationToken cancellationToken)
		{
			return await _requestReceiver.ReceiveQueryAsync(new GetProjectsQuery(), cancellationToken);
		}

		[HttpPost("{key}")]
		public async Task UploadReportAsync(string key, UploadReportBody body, CancellationToken cancellationToken)
		{
			await _requestReceiver.ReceiveCommandAsync(new UploadReportCommand { Project = key, Version = body.Version }, cancellationToken);
		}
	}

	public record UploadReportBody
	{
		public required string Version { get; init; }
	}
}
