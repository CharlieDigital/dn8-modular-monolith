using ChrlsChn.MoMo.Common.Data;
using ChrlsChn.MoMo.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChrlsChn.MoMo.Reporting.Controllers;

[ApiController]
public class ReportingController(ILogger<ReportingController> logger, TaskDatabase database)
{
    // 👇 Note here we are specifically identifying a group name
    [ApiExplorerSettings(GroupName = Constants.ReportingApiGroup)]
    [HttpDelete("/api/reporting/{projectId}", Name = nameof(GenerateProjectReportCsv))]
    public async Task<string> GenerateProjectReportCsv(Guid projectId)
    {
        logger.LogDebug("[REPORTING] Generating project report for: {ProjectId}", projectId);

        // TODO: Actual CSV report
        return await Task.FromResult("");
    }
}
