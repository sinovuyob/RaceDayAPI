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
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpGet("events/{eventId}/categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var exists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!exists) return NotFound();

        var categories = await _db.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new CategoryResponseDto(
                c.CategoryId, c.EventId, c.CategoryName, c.DistanceKm,
                c.EntryFee, c.AgeMin, c.AgeMax, c.GenderRestriction, c.Capacity,
                c.Enrolments.Count
            )).ToListAsync();

        return Ok(categories);
    }

    [HttpGet("categories/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _db.Categories
            .Include(x => x.Enrolments)
            .FirstOrDefaultAsync(x => x.CategoryId == id);

        if (c == null) return NotFound();

        return Ok(new CategoryResponseDto(
            c.CategoryId, c.EventId, c.CategoryName, c.DistanceKm,
            c.EntryFee, c.AgeMin, c.AgeMax, c.GenderRestriction, c.Capacity,
            c.Enrolments.Count
        ));
    }

    [HttpPost("events/{eventId}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _db.Events.FindAsync(eventId);

        if (ev == null) return NotFound();
        if (ev.OrganiserId != userId) return Forbid();

        var category = new Category
        {
            EventId = eventId,
            CategoryName = dto.CategoryName,
            DistanceKm = dto.DistanceKm,
            EntryFee = dto.EntryFee,
            AgeMin = dto.AgeMin,
            AgeMax = dto.AgeMax,
            GenderRestriction = dto.GenderRestriction,
            Capacity = dto.Capacity
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, new { category.CategoryId });
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null) return NotFound();
        if (category.Event.OrganiserId != userId) return Forbid();

        category.CategoryName = dto.CategoryName;
        category.DistanceKm = dto.DistanceKm;
        category.EntryFee = dto.EntryFee;
        category.AgeMin = dto.AgeMin;
        category.AgeMax = dto.AgeMax;
        category.GenderRestriction = dto.GenderRestriction;
        category.Capacity = dto.Capacity;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Category updated" });
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null) return NotFound();
        if (category.Event.OrganiserId != userId) return Forbid();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Category deleted" });
    }
}
