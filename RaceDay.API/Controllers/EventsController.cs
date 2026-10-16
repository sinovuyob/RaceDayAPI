using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Infrastructure.Data;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns all events, optionally filtered by status or location.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? location)
    {
        var query = _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.EventStatus == status);

        if (!string.IsNullOrEmpty(location))
            query = query.Where(e => e.Location.Contains(location));

        var result = await query.Select(e => new EventResponseDto(
            e.EventId, e.EventName, e.EventDate, e.Location, e.Description,
            e.EventType, e.EventStatus, e.MaxParticipants, e.RegistrationDeadline,
            e.Organiser.FullName, e.Categories.Count
        )).ToListAsync();

        return Ok(result);
    }

    /// <summary>Returns a single event by ID.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev == null) return NotFound();

        return Ok(new EventResponseDto(
            ev.EventId, ev.EventName, ev.EventDate, ev.Location, ev.Description,
            ev.EventType, ev.EventStatus, ev.MaxParticipants, ev.RegistrationDeadline,
            ev.Organiser.FullName, ev.Categories.Count
        ));
    }

    /// <summary>Creates a new event. Organisers only.</summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var ev = new Event
        {
            OrganiserId = userId,
            EventName = dto.EventName,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Description = dto.Description,
            EventType = dto.EventType,
            MaxParticipants = dto.MaxParticipants,
            RegistrationDeadline = dto.RegistrationDeadline,
            EventStatus = "Upcoming"
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new { ev.EventId });
    }

    /// <summary>Updates an event. Only the owning Organiser.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _db.Events.FindAsync(id);

        if (ev == null) return NotFound();
        if (ev.OrganiserId != userId) return Forbid();

        ev.EventName = dto.EventName;
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location;
        ev.Description = dto.Description;
        ev.EventType = dto.EventType;
        ev.MaxParticipants = dto.MaxParticipants;
        ev.RegistrationDeadline = dto.RegistrationDeadline;
        ev.EventStatus = dto.EventStatus;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Event updated" });
    }

    /// <summary>Soft-deletes an event. Only the owning Organiser.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _db.Events.FindAsync(id);

        if (ev == null) return NotFound();
        if (ev.OrganiserId != userId) return Forbid();

        ev.EventStatus = "Cancelled";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Event cancelled" });
    }

    /// <summary>Returns events created by the currently authenticated Organiser.</summary>
    [HttpGet("my-events")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetMyEvents()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var events = await _db.Events
            .Where(e => e.OrganiserId == userId)
            .Include(e => e.Categories)
            .Select(e => new EventResponseDto(
                e.EventId, e.EventName, e.EventDate, e.Location, e.Description,
                e.EventType, e.EventStatus, e.MaxParticipants, e.RegistrationDeadline,
                "you", e.Categories.Count
            )).ToListAsync();

        return Ok(events);
    }
}