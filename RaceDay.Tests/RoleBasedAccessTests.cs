using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;

namespace RaceDay.Tests;

public class RoleBasedAccessTests
{
    [Fact]
    public void EventsController_CreateMethod_RequiresOrganiserRole()
    {
        var method = typeof(EventsController).GetMethod("Create");
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().FirstOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Roles.Should().Be("Organiser");
    }

    [Fact]
    public void EventsController_UpdateMethod_RequiresOrganiserRole()
    {
        var method = typeof(EventsController).GetMethod("Update");
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().FirstOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Roles.Should().Be("Organiser");
    }

    [Fact]
    public void EventsController_DeleteMethod_RequiresOrganiserRole()
    {
        var method = typeof(EventsController).GetMethod("Delete");
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().FirstOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Roles.Should().Be("Organiser");
    }

    [Fact]
    public void CategoriesController_CreateMethod_RequiresOrganiserRole()
    {
        var method = typeof(CategoriesController).GetMethod("Create");
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().FirstOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Roles.Should().Be("Organiser");
    }

    [Fact]
    public void EnrolmentsController_EnrolMethod_RequiresAuthenticatedUser()
    {
        var method = typeof(EnrolmentsController).GetMethod("Enrol");
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().FirstOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Roles.Should().BeNullOrEmpty(); // Any authenticated user
    }

    [Fact]
    public void ResultsController_CreateMethod_RequiresOrganiserRole()
    {
        var method = typeof(ResultsController).GetMethod("Create");
        var attribute = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().FirstOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Roles.Should().Be("Organiser");
    }
}