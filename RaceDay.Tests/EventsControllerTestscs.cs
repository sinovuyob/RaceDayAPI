using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;

namespace RaceDay.Tests;

public class EventsControllerTests
{
    private static EventsController CreateControllerWithUser(RaceDay.Infrastructure.Data.RaceDayDbContext db, int userId, string role)
    {
        var controller = new EventsController(db);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
        return controller;
    }

    [Fact]
    public async Task Create_AsOrganiser_ReturnsCreated()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var controller = CreateControllerWithUser(db, 1, "Organiser");

        var dto = new CreateEventDto("Test Marathon", DateTime.UtcNow.AddMonths(1), "Cape Town", "Test", "Run", 100, null);

        var result = await controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
        db.Events.Count().Should().Be(1);
        db.Events.First().EventName.Should().Be("Test Marathon");
    }

    [Fact]
    public async Task GetAll_ReturnsAllEvents()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User { UserId = 1, Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" });
        db.Events.Add(new Event { EventId = 1, OrganiserId = 1, EventName = "Event A", EventDate = DateTime.UtcNow.AddDays(10), Location = "Durban", EventType = "Run" });
        db.Events.Add(new Event { EventId = 2, OrganiserId = 1, EventName = "Event B", EventDate = DateTime.UtcNow.AddDays(20), Location = "Soweto", EventType = "Cycle" });
        await db.SaveChangesAsync();

        var controller = CreateControllerWithUser(db, 1, "Organiser");
        var result = await controller.GetAll(null, null);

        result.Should().BeOfType<OkObjectResult>();
        var list = ((OkObjectResult)result).Value as IEnumerable<EventResponseDto>;
        list.Should().NotBeNull();
        list!.Count().Should().Be(2);
    }

    [Fact]
    public async Task Update_ByNonOwningOrganiser_ReturnsForbid()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User { UserId = 1, Email = "org1@test.com", PasswordHash = "x", FullName = "Org1", Role = "Organiser" });
        db.Events.Add(new Event { EventId = 1, OrganiserId = 1, EventName = "Event A", EventDate = DateTime.UtcNow.AddDays(10), Location = "Durban", EventType = "Run" });
        await db.SaveChangesAsync();

        // Organiser 2 tries to update Organiser 1's event
        var controller = CreateControllerWithUser(db, 2, "Organiser");
        var dto = new UpdateEventDto("Hacked", DateTime.UtcNow.AddDays(15), "Nowhere", "Hacked", "Walk", 50, null, "Upcoming");

        var result = await controller.Update(1, dto);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Delete_ByOwningOrganiser_SoftDeletesEvent()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User { UserId = 1, Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" });
        db.Events.Add(new Event { EventId = 1, OrganiserId = 1, EventName = "Event A", EventDate = DateTime.UtcNow.AddDays(10), Location = "Durban", EventType = "Run", EventStatus = "Upcoming" });
        await db.SaveChangesAsync();

        var controller = CreateControllerWithUser(db, 1, "Organiser");
        var result = await controller.Delete(1);

        result.Should().BeOfType<OkObjectResult>();
        db.Events.First().EventStatus.Should().Be("Cancelled");
    }
}