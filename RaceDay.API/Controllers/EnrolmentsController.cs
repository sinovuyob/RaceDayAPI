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
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpPost("events/{eventId}/enrol")]
    [Authorize]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] CreateEnrolmentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found" });

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId && c.EventId == eventId);
        if (category == null) return NotFound(new { message = "Category not found for this event" });

        var alreadyEnrolled = await _db.Enrolments
            .AnyAsync(e => e.ParticipantId == userId && e.EventId == eventId);
        if (alreadyEnrolled) return Conflict(new { message = "Already enrolled in this event" });

        if (category.Capacity.HasValue)
        {
            var count = await _db.Enrolments.CountAsync(e => e.CategoryId == category.CategoryId);
            if (count >= category.Capacity.Value)
                return BadRequest(new { message = "Category is full" });
        }

        var enrolment = new Enrolment
        {
            ParticipantId = userId,
            CategoryId = dto.CategoryId,
            EventId = eventId,
            Status = "Registered",
            PaymentStatus = "Pending"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { enrolmentId = enrolment.EnrolmentId });
    }

    [HttpGet("enrolments/{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var enrolment = await _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment == null) return NotFound();

        return Ok(MapToDto(enrolment));
    }

    [HttpDelete("enrolments/{id}")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var enrolment = await _db.Enrolments.FindAsync(id);

        if (enrolment == null) return NotFound();
        if (enrolment.ParticipantId != userId) return Forbid();

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Enrolment cancelled" });
    }

    [HttpGet("events/{eventId}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _db.Events.FindAsync(eventId);

        if (ev == null) return NotFound();
        if (ev.OrganiserId != userId) return Forbid();

        var enrolments = await _db.Enrolments
            .Where(e => e.EventId == eventId)
            .Include(e => e.Participant)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Select(e => MapToDto(e))
            .ToListAsync();

        return Ok(enrolments);
    }

    [HttpGet("me/enrolments")]
    [Authorize]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrolments = await _db.Enrolments
            .Where(e => e.ParticipantId == userId)
            .Include(e => e.Participant)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Select(e => MapToDto(e))
            .ToListAsync();

        return Ok(enrolments);
    }

    private static EnrolmentResponseDto MapToDto(Enrolment e) => new(
        e.EnrolmentId,
        e.ParticipantId,
        e.Participant.FullName,
        e.EventId,
        e.Event.EventName,
        e.CategoryId,
        e.Category.CategoryName,
        e.EntryDate,
        e.Status,
        e.BibNumber,
        e.PaymentStatus
    );
}