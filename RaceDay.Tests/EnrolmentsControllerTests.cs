using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;

namespace RaceDay.Tests;

public class EnrolmentsControllerTests
{
    private static EnrolmentsController CreateControllerWithUser(RaceDay.Infrastructure.Data.RaceDayDbContext db, int userId, string role)
    {
        var controller = new EnrolmentsController(db);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    [Fact]
    public async Task Enrol_AsParticipant_CreatesEnrolment()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User { UserId = 1, Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" });
        db.Users.Add(new User { UserId = 2, Email = "part@test.com", PasswordHash = "x", FullName = "Part", Role = "Participant" });
        db.Events.Add(new Event { EventId = 1, OrganiserId = 1, EventName = "Comrades", EventDate = DateTime.UtcNow.AddMonths(2), Location = "PMB", EventType = "Run" });
        db.Categories.Add(new Category { CategoryId = 1, EventId = 1, CategoryName = "90km", DistanceKm = 90, EntryFee = 1200 });
        await db.SaveChangesAsync();

        var controller = CreateControllerWithUser(db, 2, "Participant");
        var result = await controller.Enrol(1, new CreateEnrolmentDto(1));

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(201);
        db.Enrolments.Count().Should().Be(1);
        db.Enrolments.First().ParticipantId.Should().Be(2);
        db.Enrolments.First().CategoryId.Should().Be(1);
        db.Enrolments.First().EventId.Should().Be(1);
    }

    [Fact]
    public async Task Enrol_TwiceForSameEvent_ReturnsConflict()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User { UserId = 1, Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" });
        db.Users.Add(new User { UserId = 2, Email = "part@test.com", PasswordHash = "x", FullName = "Part", Role = "Participant" });
        db.Events.Add(new Event { EventId = 1, OrganiserId = 1, EventName = "Comrades", EventDate = DateTime.UtcNow.AddMonths(2), Location = "PMB", EventType = "Run" });
        db.Categories.Add(new Category { CategoryId = 1, EventId = 1, CategoryName = "90km", DistanceKm = 90, EntryFee = 1200 });
        await db.SaveChangesAsync();

        var controller = CreateControllerWithUser(db, 2, "Participant");
        await controller.Enrol(1, new CreateEnrolmentDto(1));

        // Second attempt
        var result = await controller.Enrol(1, new CreateEnrolmentDto(1));

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Enrol_IntoFullCategory_ReturnsBadRequest()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User { UserId = 1, Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" });
        db.Users.Add(new User { UserId = 2, Email = "p1@test.com", PasswordHash = "x", FullName = "P1", Role = "Participant" });
        db.Users.Add(new User { UserId = 3, Email = "p2@test.com", PasswordHash = "x", FullName = "P2", Role = "Participant" });
        db.Events.Add(new Event { EventId = 1, OrganiserId = 1, EventName = "Comrades", EventDate = DateTime.UtcNow.AddMonths(2), Location = "PMB", EventType = "Run" });
        db.Categories.Add(new Category { CategoryId = 1, EventId = 1, CategoryName = "90km", DistanceKm = 90, EntryFee = 1200, Capacity = 1 });
        await db.SaveChangesAsync();

        // First participant takes the only slot
        var controller1 = CreateControllerWithUser(db, 2, "Participant");
        await controller1.Enrol(1, new CreateEnrolmentDto(1));

        // Second participant tries but category is full
        var controller2 = CreateControllerWithUser(db, 3, "Participant");
        var result = await controller2.Enrol(1, new CreateEnrolmentDto(1));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cancel_OwnEnrolment_UpdatesStatus()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User { UserId = 1, Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" });
        db.Users.Add(new User { UserId = 2, Email = "part@test.com", PasswordHash = "x", FullName = "Part", Role = "Participant" });
        db.Events.Add(new Event { EventId = 1, OrganiserId = 1, EventName = "Comrades", EventDate = DateTime.UtcNow.AddMonths(2), Location = "PMB", EventType = "Run" });
        db.Categories.Add(new Category { CategoryId = 1, EventId = 1, CategoryName = "90km", DistanceKm = 90, EntryFee = 1200 });
        await db.SaveChangesAsync();

        var controller = CreateControllerWithUser(db, 2, "Participant");
        await controller.Enrol(1, new CreateEnrolmentDto(1));
        var enrolmentId = db.Enrolments.First().EnrolmentId;

        var result = await controller.Cancel(enrolmentId);

        result.Should().BeOfType<OkObjectResult>();
        db.Enrolments.First().Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Cancel_SomeoneElsesEnrolment_ReturnsForbid()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Users.Add(new User { UserId = 1, Email = "org@test.com", PasswordHash = "x", FullName = "Org", Role = "Organiser" });
        db.Users.Add(new User { UserId = 2, Email = "p1@test.com", PasswordHash = "x", FullName = "P1", Role = "Participant" });
        db.Users.Add(new User { UserId = 3, Email = "p2@test.com", PasswordHash = "x", FullName = "P2", Role = "Participant" });
        db.Events.Add(new Event { EventId = 1, OrganiserId = 1, EventName = "Comrades", EventDate = DateTime.UtcNow.AddMonths(2), Location = "PMB", EventType = "Run" });
        db.Categories.Add(new Category { CategoryId = 1, EventId = 1, CategoryName = "90km", DistanceKm = 90, EntryFee = 1200 });
        await db.SaveChangesAsync();

        var controller1 = CreateControllerWithUser(db, 2, "Participant");
        await controller1.Enrol(1, new CreateEnrolmentDto(1));
        var enrolmentId = db.Enrolments.First().EnrolmentId;

        // Participant 3 tries to cancel Participant 2's enrolment
        var controller3 = CreateControllerWithUser(db, 3, "Participant");
        var result = await controller3.Cancel(enrolmentId);

        result.Should().BeOfType<ForbidResult>();
    }
}