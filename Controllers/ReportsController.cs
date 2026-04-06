using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prokat.API.Services;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IClientReportService _reports;

        public ReportsController(IClientReportService reports)
        {
            _reports = reports;
        }

        [HttpGet("clients")]
        public async Task<IActionResult> ClientReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        {
            var rows = await _reports.GetReportAsync(from, to, ct);
            return Ok(rows);
        }
    }
}
