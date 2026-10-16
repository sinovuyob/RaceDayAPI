using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Controllers;
using RaceDay.Core.DTOs;
using RaceDay.Infrastructure.Services;

namespace RaceDay.Tests;

public class AuthControllerTests
{
    private static IConfiguration BuildConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Key"] = "ThisIsAVerySecretKeyForRaceDayAppThatIsAtLeast32CharsLong!",
                ["Jwt:Issuer"] = "RaceDayAPI",
                ["Jwt:Audience"] = "RaceDayClient",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDb();
        var controller = new AuthController(db, new TokenService(BuildConfig()));
        var dto = new RegisterDto("new@test.com", "Password123!", "New User", "Participant", null, null);

        // Act
        var result = await controller.Register(dto);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var obj = (ObjectResult)result;
        obj.StatusCode.Should().Be(201);
        obj.Value.Should().BeOfType<AuthResponseDto>();

        db.Users.Count().Should().Be(1);
        db.Users.First().PasswordHash.Should().NotBe("Password123!");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDb();
        var controller = new AuthController(db, new TokenService(BuildConfig()));
        var dto = new RegisterDto("dup@test.com", "Password123!", "First", "Participant", null, null);

        await controller.Register(dto); // First registration succeeds

        // Act — same email again
        var result = await controller.Register(dto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var controller = new AuthController(db, new TokenService(BuildConfig()));
        var dto = new RegisterDto("badrole@test.com", "Password123!", "Bad Role", "Admin", null, null);

        var result = await controller.Register(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var controller = new AuthController(db, new TokenService(BuildConfig()));

        await controller.Register(new RegisterDto("user@test.com", "Password123!", "User", "Participant", null, null));

        // Act
        var result = await controller.Login(new LoginDto("user@test.com", "Password123!"));

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        var auth = ok.Value as AuthResponseDto;
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrEmpty();
        auth.User.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var controller = new AuthController(db, new TokenService(BuildConfig()));

        await controller.Register(new RegisterDto("user2@test.com", "Password123!", "User", "Participant", null, null));

        // Act
        var result = await controller.Login(new LoginDto("user2@test.com", "WrongPassword"));

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithNonexistentEmail_ReturnsUnauthorized()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var controller = new AuthController(db, new TokenService(BuildConfig()));

        var result = await controller.Login(new LoginDto("nobody@test.com", "Password123!"));

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}