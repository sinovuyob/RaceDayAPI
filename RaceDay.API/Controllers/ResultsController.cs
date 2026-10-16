using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Infrastructure.Data;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpPost("enrolments/{enrolmentId}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int enrolmentId, [FromBody] CreateResultDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrolment = await _db.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);
        if (enrolment == null) return NotFound();

        if (enrolment.Event.OrganiserId != userId) return Forbid();

        var existing = await _db.Results
            .FirstOrDefaultAsync(r => r.EnrolmentId == enrolmentId);
        if (existing != null) return Conflict(new { message = "Result already exists for this enrolment" });

        var result = new Result
        {
            EnrolmentId = enrolmentId,
            FinishTime = dto.FinishTime,
            OverallPosition = dto.OverallPosition,
            CategoryPosition = dto.CategoryPosition,
            Status = "Pending"
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { resultId = result.ResultId });
    }

    [HttpGet("results/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Category)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null) return NotFound();

        return Ok(MapToDto(result));
    }

    [HttpPut("results/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResultDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null) return NotFound();
        if (result.Enrolment.Event.OrganiserId != userId) return Forbid();

        result.FinishTime = dto.FinishTime;
        result.OverallPosition = dto.OverallPosition;
        result.CategoryPosition = dto.CategoryPosition;
        result.Status = dto.Status;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Result updated" });
    }

    [HttpGet("events/{eventId}/results")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var results = await _db.Results
            .Where(r => r.Enrolment.EventId == eventId)
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Category)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return Ok(results);
    }

    private static ResultResponseDto MapToDto(Result r) => new(
        r.ResultId,
        r.EnrolmentId,
        r.Enrolment.Participant.FullName,
        r.Enrolment.Event.EventName,
        r.Enrolment.Category.CategoryName,
        r.FinishTime,
        r.OverallPosition,
        r.CategoryPosition,
        r.Status
    );
}