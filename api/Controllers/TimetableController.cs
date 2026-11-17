using api.Interfaces;
using api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/timetable")]
public sealed class TimetableController(ITimetableRepository timetableService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TimetableDto>>> List([FromQuery] Guid? business, [FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        return Ok(await timetableService.ListAsync(business, date, cancellationToken));
    }
}
